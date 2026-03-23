using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDjango.Admin.Services;
using Newtonsoft.Json;
using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    /// <summary>
    /// Mutation tests for SubstringFilter.
    /// Targets NoCoverage mutants from Stryker run 2026-03-17.
    ///
    /// Equivalent mutants (unkillable):
    ///   - id=148 (Single → SingleOrDefault): Exactly one private Apply&lt;T&gt; method exists;
    ///     both return the same result.
    ///   - id=150 (&amp;&amp; → || in method predicate): public Apply is not NonPublic, so the
    ///     GetMethods result is identical with either operator.
    ///   - id=164 (ConfigureAwait(false) → true in while loop ReadAsync): No SynchronizationContext
    ///     in xUnit; scheduling behaviour is the same.
    ///   - id=169 (ConfigureAwait(false) → true in ReadAsStringAsync): Same reasoning as id=164.
    ///   - id=171 (ConfigureAwait(false) → true in SkipAsync): Same reasoning as id=164.
    ///   - id=174 (break removed at EndObject): For simple JSON objects without trailing content,
    ///     the next ReadAsync returns false, exiting the while loop identically.
    ///   - id=170 (SkipAsync removed): For single-level unknown properties with scalar values, the
    ///     next ReadAsync in the while condition advances past the scalar automatically.
    ///   - id=187 (false → true when attr == null): All mapped Category properties have a
    ///     corresponding MetaEntityAttr after LoadFromDbContext; attr is never null.
    /// </summary>
    public class SubstringFilterTests
    {
        private readonly MetaData _meta;
        private readonly MetaEntity _categoryEntity;
        private readonly List<Category> _categories;

        public SubstringFilterTests()
        {
            var dbContext = TestDbContext.Create();
            _meta = new MetaData();
            _meta.LoadFromDbContext(dbContext);
            _categoryEntity = _meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            _categories = new List<Category>
            {
                new() { Id = 1, CategoryName = "Beverages", Description = "Soft drinks, coffees, teas" },
                new() { Id = 2, CategoryName = "Condiments", Description = "Sweet and savory sauces" },
                new() { Id = 3, CategoryName = "Dairy Products", Description = "Cheeses" },
            };
        }

        private static async Task ReadFilterAsync(SubstringFilter filter, string json)
        {
            using var sr = new StringReader(json);
            using var reader = new JsonTextReader(sr);
            await filter.ReadFromJsonAsync(reader);
        }

        private static readonly string[] s_categoryNameField = ["CategoryName"];

        private static void SetSearchFields(MetaEntity entity, IReadOnlyList<string> fields)
        {
            typeof(MetaEntity)
                .GetProperty("SearchFields")
                .SetValue(entity, fields);
        }

        private List<Category> ApplyFilter(SubstringFilter filter, bool isLookup = false)
        {
            return ((IQueryable<Category>)filter.Apply(_categoryEntity, isLookup, _categories.AsQueryable())).ToList();
        }

        // ─── Apply: early return on empty filter text ─────────────────────────────

        [Fact]
        public void Apply_NullFilterText_ReturnsAllItems()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            // _filterText is null (ReadFromJsonAsync never called)

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=145 (null!="" → true → falls through → null passed to FullTextSearchQuery → Split() throws NRE),
            //          kills id=146 (null.Trim() → NullReferenceException before comparison),
            //          kills id=147 (negate → !IsNullOrWhiteSpace(null)=false → skips early return → throws)
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task Apply_WhitespaceFilterText_ReturnsAllItems()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"   "}""");

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=144 ("   "!=null → true → falls through → FullTextSearchQuery("   ") →
            //            texts list is empty after Trim/Where → Exp.AndAlso(notNullExp, null) throws)
            Assert.Equal(3, result.Count);
        }

        // ─── Apply: non-empty filter text triggers reflection-based filtering ─────

        [Fact]
        public async Task Apply_WithMatchingFilterText_ReturnsFilteredItems()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");
            SetSearchFields(_categoryEntity, s_categoryNameField);

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=149 (BindingFlags & instead of | → GetMethods(0) returns nothing → Single throws),
            //          kills id=151 (m.Name!="Apply" → no match → Single throws InvalidOperationException),
            //          kills id=152 ("Apply"→"" → no method named "" → Single throws),
            //          kills id=153 (empty args array → Invoke throws TargetParameterCountException),
            //          kills id=154 (Apply<T> body removed → returns null → IQueryable<Category> cast or ToList throws)
            Assert.Single(result);
            Assert.Equal("Beverages", result[0].CategoryName);
        }

        // ─── ReadFromJsonAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task ReadFromJsonAsync_ValidJsonWithValue_SetsFilterText()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            SetSearchFields(_categoryEntity, s_categoryNameField);

            // Act
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");
            var result = ApplyFilter(filter);

            // Assert – kills id=158 (!await→await → ReadAsync returns true → condition becomes true → throws
            //            even for valid JSON),
            //          kills id=163 (negate while → loop never executes → filterText stays null → all 3 returned)
            Assert.Single(result);
        }

        [Fact]
        public async Task ReadFromJsonAsync_NotStartObject_ThrowsBadJsonFormatException()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            using var sr = new StringReader("\"just a string\"");
            using var reader = new JsonTextReader(sr);

            // Act & Assert – kills id=156 (&&→|| → false && true = false → does not throw),
            //                kills id=157 (negate whole condition → !(false||true)=false → does not throw),
            //                kills id=160 (!=StartObject → ==StartObject → String==StartObject is false → does not throw),
            //                kills id=162 (throw removed → no BadJsonFormatException raised)
            await Assert.ThrowsAsync<BadJsonFormatException>(() =>
                filter.ReadFromJsonAsync(reader));
        }

        [Fact]
        public async Task ReadFromJsonAsync_EmptyStream_ThrowsBadJsonFormatException()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            using var sr = new StringReader(string.Empty);
            using var reader = new JsonTextReader(sr);

            // Act & Assert – kills id=159 (false→true → condition = !ReadAsync(false)||true = true||true → always throws,
            //                  but id=159 is only distinguished here because with valid JSON
            //                  the ReadFromJsonAsync_ValidJsonWithValue test would also throw unexpectedly)
            await Assert.ThrowsAsync<BadJsonFormatException>(() =>
                filter.ReadFromJsonAsync(reader));
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithUnknownProperty_StillSetsFilterText()
        {
            // Arrange
            var filter = new SubstringFilter(_meta);
            SetSearchFields(_categoryEntity, s_categoryNameField);

            // Act
            await ReadFilterAsync(filter, """{"unknown":"ignored","value":"Beverages"}""");
            var result = ApplyFilter(filter);

            // Assert – kills id=166 (PropertyName!= → PropertyName== → "unknown" triggers value case,
            //            "value" falls to else branch → filterText never set → all 3 returned)
            Assert.Single(result);
        }

        // ─── GetFilterOptions: searchFields branch ───────────────────────────────

        [Fact]
        public async Task Apply_WithSearchFieldsMatchingField_ReturnsMatches()
        {
            // Arrange – "Beverages" is in CategoryName which is listed in searchFields
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");
            SetSearchFields(_categoryEntity, s_categoryNameField);

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=184 (Contains block removed → filter always returns false →
            //            predicateBody null → all 3 items returned unfiltered)
            Assert.Single(result);
        }

        [Fact]
        public async Task Apply_WithSearchFields_ExcludesFieldsNotInList()
        {
            // Arrange – "Soft" is only in Description, not in CategoryName
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Soft"}""");
            SetSearchFields(_categoryEntity, s_categoryNameField);

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=176 (empty FullTextSearchOptions → Filter=null → all props searched → 1 result),
            //          kills id=179 (negate searchFields condition → uses legacy → Description included → 1 result),
            //          kills id=180 (!=null → ==null → non-null searchFields treated as null → legacy includes Description)
            Assert.Empty(result);
        }

        [Fact]
        public async Task Apply_WithEmptySearchFields_FallsBackToLegacyFilter()
        {
            // Arrange – Count=0 → 0>0 is false → falls through to legacy; mutant id=182 uses Count>=0=true
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");
            SetSearchFields(_categoryEntity, Array.Empty<string>());

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=182 (Count>0 → Count>=0 → empty list enters Contains branch →
            //            Contains("Beverages") always false → predicateBody null → all 3 items returned)
            Assert.Single(result);
        }

        [Fact]
        public async Task Apply_WithNullSearchFields_FallsBackToLegacyFilter()
        {
            // Arrange – SearchFields is null by default (no IAdminSettings configured)
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=178 (||→&& → null!=null is false; then null.Count>0 → NullReferenceException)
            Assert.Single(result);
        }

        // ─── GetFilterOptions: legacy fallback (IsVisible / ShowOnView) ──────────

        [Fact]
        public async Task Apply_LegacyFallback_ExcludesPropertyWithShowOnViewFalse()
        {
            // Arrange – "Soft drinks" exists only in Description; Description.ShowOnView set to false
            var descAttr = _categoryEntity.FindAttribute(a => a.PropName == "Description");
            descAttr.ShowOnView = false;

            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Soft drinks"}""");

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=188 (||→&& → !IsVisible&&!ShowOnView → false&&true=false → includes Description → 1 result),
            //          kills id=189 (negate → IsVisible&&ShowOnView → true&&false=false → includes Description → 1 result),
            //          kills id=191 (!ShowOnView → ShowOnView → false → don't exclude → Description included → 1 result),
            //          kills id=192 (false→true → returns true even when not visible → Description included → 1 result)
            Assert.Empty(result);
        }

        [Fact]
        public async Task Apply_LegacyFallback_ExcludesPropertyWithIsVisibleFalse()
        {
            // Arrange – "Beverages" exists only in CategoryName; CategoryName.IsVisible set to false
            var catNameAttr = _categoryEntity.FindAttribute(a => a.PropName == "CategoryName");
#pragma warning disable CS0618 // IsVisible is obsolete but still checked in SubstringFilter source
            catNameAttr.IsVisible = false;
#pragma warning restore CS0618

            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");

            // Act
            var result = ApplyFilter(filter);

            // Assert – kills id=188 (||→&& → !IsVisible&&!ShowOnView → true&&false=false → includes CategoryName → 1 result),
            //          kills id=190 (!IsVisible → IsVisible → false → don't exclude → CategoryName included → 1 result)
            Assert.Empty(result);
        }

        // ─── GetFilterOptions: legacy fallback (isLookup) ───────────────────────

        [Fact]
        public async Task Apply_LegacyFallback_NonLookupMode_SearchesDescriptionProperty()
        {
            // Arrange – isLookup=false → Description IS searched (ShowOnView=true)
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Soft drinks"}""");

            // Act
            var result = ApplyFilter(filter, isLookup: false);

            // Assert – kills id=193 (&&→|| → isLookup=false, but !IsPrimaryKey=true → Description excluded → 0 results),
            //          kills id=195 (isLookup&&!ShowInLookup → isLookup||!ShowInLookup → false||true → Description excluded → 0),
            //          kills id=199 (true→false at end of filter → always returns false → predicateBody null → all 3 returned)
            Assert.Single(result);
            Assert.Equal("Beverages", result[0].CategoryName);
        }

        [Fact]
        public async Task Apply_LegacyFallback_LookupMode_ExcludesNonLookupProperty()
        {
            // Arrange – isLookup=true → Description excluded (ShowInLookup=false, IsPrimaryKey=false)
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Soft drinks"}""");

            // Act
            var result = ApplyFilter(filter, isLookup: true);

            // Assert – kills id=185 (PropInfo==prop → PropInfo!=prop → Description gets wrong attr with IsPrimaryKey=true → included → 1 result),
            //          kills id=186 (attr==null → attr!=null → returns false for all found attrs → predicateBody null → all 3 returned),
            //          kills id=194 (negate isLookup condition → includes when should exclude → "Soft drinks" found → 1 result),
            //          kills id=198 (false→true → returns true when isLookup condition fires → Description included → 1 result)
            Assert.Empty(result);
        }

        [Fact]
        public async Task Apply_LegacyFallback_LookupMode_IncludesPrimaryKeyProperty()
        {
            // Arrange – isLookup=true → Id (IsPrimaryKey=true) IS searched; Id=1 → "1".Contains("1")
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"1"}""");

            // Act
            var result = ApplyFilter(filter, isLookup: true);

            // Assert – kills id=197 (!IsPrimaryKey → IsPrimaryKey → condition fires for PK → excludes Id → 0 results)
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task Apply_LegacyFallback_LookupMode_IncludesShowInLookupProperty()
        {
            // Arrange – isLookup=true → CategoryName (ShowInLookup=true set by loader) IS searched
            var filter = new SubstringFilter(_meta);
            await ReadFilterAsync(filter, """{"value":"Beverages"}""");

            // Act
            var result = ApplyFilter(filter, isLookup: true);

            // Assert – kills id=196 (!ShowInLookup → ShowInLookup → condition fires for ShowInLookup=true → excludes CategoryName → 0 results),
            //          kills id=195 (isLookup||!ShowInLookup → true||false=true → condition fires → CategoryName excluded → 0 results)
            Assert.Single(result);
            Assert.Equal("Beverages", result[0].CategoryName);
        }
    }
}
