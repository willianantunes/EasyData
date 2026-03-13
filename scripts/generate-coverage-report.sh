#!/bin/bash
# Usage:
#   dotnet test ... | bash ./scripts/generate-coverage-report.sh [filter]
#   docker compose run ... | bash ./scripts/generate-coverage-report.sh [filter]
#
# Reads test output from stdin, extracts coverage.cobertura.xml paths from the
# "Attachments:" section, merges all Cobertura files, and prints a per-file
# coverage report. An optional filter argument does a case-insensitive match on
# the source file path.
#
# Handles both local paths and Docker /app/ paths transparently.
#
# ── Design decisions ─────────────────────────────────────────────────────────
#
# Why Cobertura XML?
#   runsettings.xml emits five formats (json, cobertura, lcov, teamcity, opencover).
#   Cobertura was chosen because it is line-oriented XML that awk can stream-parse
#   without a real XML library, and it includes both line hits and branch coverage
#   in a single attribute (condition-coverage="50% (1/2)").
#
# Why parse from stdin instead of discovering files on disk?
#   Each `dotnet test` run produces coverage under a random GUID directory.
#   The test runner prints the exact paths in its "Attachments:" footer, so
#   extracting them from stdout avoids fragile glob-based discovery and
#   guarantees we read coverage from the run that just finished — not stale
#   results from a previous run.
#
# Path normalization — why <source> + filename?
#   Cobertura `filename` attributes are relative to the `<source>` element.
#   Different test projects produce different source roots:
#     - Docker:  <source>/app/src/</source>               filename="NDjango.Admin.Core/Entities.cs"
#     - Docker:  <source>/app/src/NDjango.Admin.Core/</source>  filename="Entities.cs"
#     - Local:   <source>/Users/.../src/</source>          filename="NDjango.Admin.Core/Entities.cs"
#   We concatenate source + filename, then strip everything up to "src/" to get
#   a canonical relative path ("NDjango.Admin.Core/Entities.cs") that deduplicates
#   correctly regardless of execution environment.
#
# Per-line deduplication — why track individual line numbers?
#   A single .cs file can appear as multiple <class> entries (one per C# class in
#   the file) within the same XML, AND the same file can appear across multiple
#   Cobertura XMLs from different test projects. Tracking (file, line_number) with
#   max-hits semantics handles both cases: non-overlapping lines from different
#   classes are accumulated naturally, and overlapping lines across XMLs keep the
#   best hit count.
#
# BSD awk compatibility
#   macOS ships BSD awk, not gawk. We avoid gawk-only features:
#     - No match() with capture groups (third argument)
#     - No IGNORECASE — case-insensitive filtering shells out to `tr`
#     - No gensub() — use sub()/substr()/index() instead
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

FILTER="${1:-}"

# ── 1. Capture stdin and extract Cobertura XML paths ─────────────────────────
# Stdin is fully buffered so we can scan it for attachment paths.
# The test runner output is intentionally discarded — only the coverage report
# is printed. Pipe through filter-failed-tests.sh first if you need test output.
input="$(cat)"

# Scan for paths ending in coverage.cobertura.xml. The test runner prints them
# indented under "Attachments:" — we match the filename anywhere on the line
# and then verify the file exists on disk (filters out Docker-only /app/ paths
# when running locally, and vice versa).
cobertura_files=()
while IFS= read -r line; do
    path="$(echo "$line" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"
    [ -f "$path" ] && cobertura_files+=("$path")
done < <(echo "$input" | grep -oE '[^ ]*coverage\.cobertura\.xml')

if [ ${#cobertura_files[@]} -eq 0 ]; then
    echo ""
    echo "============================================================"
    echo "  COVERAGE: No coverage.cobertura.xml files found."
    echo "  Make sure you run with: --settings \"./runsettings.xml\""
    echo "============================================================"
    exit 0
fi

# ── 2. Parse Cobertura XML files and produce the report ──────────────────────
# All Cobertura files are passed as positional arguments to a single awk
# invocation. awk processes them sequentially, picking up each file's <source>
# before its <class> entries.
awk -v filter="$FILTER" '
BEGIN {
    file_count = 0
    source_path = ""
}

# Each Cobertura XML declares a <source> base path. Filenames inside <class>
# are relative to it. We capture it here so we can build full paths below.
/<source>/ {
    s = $0
    sub(/.*<source>/, "", s)
    sub(/<\/source>.*/, "", s)
    if (s != "" && substr(s, length(s)) != "/") s = s "/"
    source_path = s
    next
}

/<class / {
    fname = ""
    if (match($0, /filename="[^"]*"/)) {
        fname = substr($0, RSTART+10, RLENGTH-11)
    }
    if (fname == "") next

    # Canonical path: source_path + filename, then drop the prefix through "src/".
    # This collapses all environment variants into one key, e.g.:
    #   /app/src/NDjango.Admin.Core/Entities.cs  ->  NDjango.Admin.Core/Entities.cs
    #   /Users/.../src/NDjango.Admin.Core/Entities.cs  ->  NDjango.Admin.Core/Entities.cs
    full = source_path fname
    if (match(full, /src\//)) {
        full = substr(full, RSTART+4)
    }

    # Case-insensitive filter via shell `tr` (BSD awk has no tolower/IGNORECASE)
    if (filter != "") {
        cmd = "printf \"%s\" \"" full "\" | tr \"[:upper:]\" \"[:lower:]\""
        cmd | getline lf
        close(cmd)
        cmd = "printf \"%s\" \"" filter "\" | tr \"[:upper:]\" \"[:lower:]\""
        cmd | getline lpat
        close(cmd)
        if (index(lf, lpat) == 0) next
    }

    in_class = 1
    class_fname = full
    next
}

in_class && /<line / {
    lnum = ""
    if (match($0, /number="[^"]*"/)) {
        lnum = substr($0, RSTART+8, RLENGTH-9) + 0
    }
    hits = 0
    if (match($0, /hits="[^"]*"/)) {
        hits = substr($0, RSTART+6, RLENGTH-7) + 0
    }

    # Keyed by (file, line_number). Using max-hits means:
    #   - Lines from different classes within one XML (disjoint sets) are stored independently.
    #   - Lines that appear in multiple XMLs (same line covered by different test projects)
    #     keep the highest hit count — i.e., "was this line exercised by any test suite?".
    key = class_fname "\t" lnum
    if (key in line_hits) {
        if (hits > line_hits[key]) line_hits[key] = hits
    } else {
        line_hits[key] = hits
    }

    # Branch coverage: Cobertura encodes it per-line as condition-coverage="50% (1/2)".
    # We extract the (covered/total) pair from inside the parentheses — the percentage
    # is redundant. Same max-hits dedup strategy as line hits.
    if (match($0, /condition-coverage="[^"]*"/)) {
        cc = substr($0, RSTART+21, RLENGTH-22)
        # Parse "(covered/total)" without gawk capture groups
        if (match(cc, /\(/)) {
            inner = substr(cc, RSTART+1)
            sub(/\)/, "", inner)
            slash = index(inner, "/")
            if (slash > 0) {
                bc = substr(inner, 1, slash-1) + 0
                bv = substr(inner, slash+1) + 0
                bkey_c = key "\tb_covered"
                bkey_v = key "\tb_valid"
                if (bkey_c in branch_data) {
                    if (bc > branch_data[bkey_c]) branch_data[bkey_c] = bc
                } else {
                    branch_data[bkey_c] = bc
                }
                if (!(bkey_v in branch_data) || bv > branch_data[bkey_v]) {
                    branch_data[bkey_v] = bv
                }
            }
        }
    }

    if (!(class_fname in file_seen)) {
        file_seen[class_fname] = 1
        file_order[file_count] = class_fname
        file_count++
    }
}

in_class && /<\/class>/ {
    in_class = 0
}

END {
    if (file_count == 0) {
        if (filter != "") {
            printf "\n============================================================\n"
            printf "  COVERAGE: No files matching \"%s\"\n", filter
            printf "============================================================\n"
        }
        exit 0
    }

    # Roll up per-line data into per-file totals. A line is "covered" if its
    # max hits across all sources is > 0.
    for (key in line_hits) {
        split(key, parts, "\t")
        f = parts[1]
        file_lv[f] += 1
        if (line_hits[key] > 0) file_lc[f] += 1
    }
    for (bkey in branch_data) {
        split(bkey, parts, "\t")
        f = parts[1]
        typ = parts[3]
        if (typ == "b_covered") file_bc[f] += branch_data[bkey]
        if (typ == "b_valid")   file_bv[f] += branch_data[bkey]
    }

    # Sort files by path (insertion sort)
    for (i = 1; i < file_count; i++) {
        key = file_order[i]
        j = i - 1
        while (j >= 0 && file_order[j] > key) {
            file_order[j+1] = file_order[j]
            j--
        }
        file_order[j+1] = key
    }

    total_lv = 0; total_lc = 0; total_bv = 0; total_bc = 0

    printf "\n"
    printf "============================================================\n"
    printf "                   CODE COVERAGE REPORT                     \n"
    printf "============================================================\n"

    if (filter != "") {
        printf "  Filter: %s\n", filter
        printf "------------------------------------------------------------\n"
    }

    printf "\n"
    printf "%-80s  %7s  %7s  %7s\n", "File", "Lines", "Branch", "Covered"
    printf "%-80s  %7s  %7s  %7s\n", "----", "-----", "------", "-------"

    for (i = 0; i < file_count; i++) {
        f = file_order[i]
        lv = (f in file_lv) ? file_lv[f] : 0
        lc = (f in file_lc) ? file_lc[f] : 0
        bv = (f in file_bv) ? file_bv[f] : 0
        bc = (f in file_bc) ? file_bc[f] : 0

        total_lv += lv; total_lc += lc
        total_bv += bv; total_bc += bc

        line_pct = (lv > 0) ? (lc / lv) * 100 : 0
        branch_pct = (bv > 0) ? (bc / bv) * 100 : -1

        covered_str = sprintf("%d/%d", lc, lv)

        if (branch_pct >= 0) {
            printf "%-80s  %6.1f%%  %6.1f%%  %s\n", f, line_pct, branch_pct, covered_str
        } else {
            printf "%-80s  %6.1f%%  %7s  %s\n", f, line_pct, "-", covered_str
        }
    }

    printf "\n"
    printf "------------------------------------------------------------\n"

    total_line_pct = (total_lv > 0) ? (total_lc / total_lv) * 100 : 0
    total_branch_pct = (total_bv > 0) ? (total_bc / total_bv) * 100 : -1

    if (total_branch_pct >= 0) {
        printf "%-80s  %6.1f%%  %6.1f%%  %d/%d\n", "TOTAL", total_line_pct, total_branch_pct, total_lc, total_lv
    } else {
        printf "%-80s  %6.1f%%  %7s  %d/%d\n", "TOTAL", total_line_pct, "-", total_lc, total_lv
    }

    printf "============================================================\n"
    printf "  Files: %d | Lines: %d/%d (%.1f%%)", file_count, total_lc, total_lv, total_line_pct
    if (total_branch_pct >= 0) {
        printf " | Branches: %d/%d (%.1f%%)", total_bc, total_bv, total_branch_pct
    }
    printf "\n"
    printf "============================================================\n"
}
' "${cobertura_files[@]}"
