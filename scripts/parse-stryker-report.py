#!/usr/bin/env python3
"""
Parse a Stryker.NET HTML report and extract survived/uncovered mutants
for a specific source file.

Usage:
    python3 scripts/parse-stryker-report.py <report.html> [file-filter]

Examples:
    python3 scripts/parse-stryker-report.py test/.../StrykerOutput/.../mutation-report.html
    python3 scripts/parse-stryker-report.py test/.../mutation-report.html AdminDashboardMiddleware
"""

import json
import sys


def parse_report(html_path, file_filter=None):
    with open(html_path, "r") as f:
        content = f.read()

    idx = content.find("app.report = {")
    if idx == -1:
        print("ERROR: Could not find 'app.report' JSON in HTML file.")
        sys.exit(1)

    json_start = idx + len("app.report = ")

    depth = 0
    in_string = False
    escape_next = False
    end_idx = json_start
    for i in range(json_start, len(content)):
        c = content[i]
        if escape_next:
            escape_next = False
            continue
        if c == "\\" and in_string:
            escape_next = True
            continue
        if c == '"' and not escape_next:
            in_string = not in_string
            continue
        if in_string:
            continue
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                end_idx = i + 1
                break

    data = json.loads(content[json_start:end_idx])
    files = data.get("files", {})

    for fname, fdata in files.items():
        if file_filter and file_filter not in fname:
            continue

        mutants = fdata.get("mutants", [])
        survived = [m for m in mutants if m.get("status") == "Survived"]
        nocov = [m for m in mutants if m.get("status") == "NoCoverage"]
        killed = [m for m in mutants if m.get("status") == "Killed"]
        total = len(killed) + len(survived) + len(nocov)

        short_name = fname.rsplit("/", 1)[-1] if "/" in fname else fname
        print(f"File: {short_name}")
        print(f"  Total testable: {total}, Killed: {len(killed)}, Survived: {len(survived)}, NoCoverage: {len(nocov)}")
        if total > 0:
            print(f"  Score: {len(killed) * 100 / total:.1f}%")
        print()

        for m in survived:
            loc = m.get("location", {})
            start = loc.get("start", {})
            end = loc.get("end", {})
            print(f"  [SURVIVED] id={m.get('id')}  {m.get('mutatorName')}")
            print(f"    Line {start.get('line')}:{start.get('column')} - {end.get('line')}:{end.get('column')}")
            print(f"    Replacement: {m.get('replacement', 'N/A')}")
            print()

        for m in nocov:
            loc = m.get("location", {})
            start = loc.get("start", {})
            end = loc.get("end", {})
            print(f"  [NO COVERAGE] id={m.get('id')}  {m.get('mutatorName')}")
            print(f"    Line {start.get('line')}:{start.get('column')} - {end.get('line')}:{end.get('column')}")
            print(f"    Replacement: {m.get('replacement', 'N/A')}")
            print()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__.strip())
        sys.exit(1)

    html_path = sys.argv[1]
    file_filter = sys.argv[2] if len(sys.argv) > 2 else None
    parse_report(html_path, file_filter)
