using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class ValueEditorTests
    {
        #region Helpers

        private static async Task<string> SerializeEditorAsync(ValueEditor editor)
        {
            var sw = new StringWriter();
            using (var writer = new JsonTextWriter(sw))
            {
                await editor.WriteToJsonAsync(writer, new BitOptions());
            }
            return sw.ToString();
        }

        private static async Task<ValueEditor> DeserializeEditorAsync(string json)
        {
            using (var sr = new StringReader(json))
            using (var reader = new JsonTextReader(sr))
            {
                await reader.ReadAsync(); // advance to StartObject
                return await ValueEditor.ReadFromJsonAsync(reader);
            }
        }

        /// <summary>
        /// Ensures the static BasicValueEditorsCreator is registered by touching MetaData's static constructor.
        /// </summary>
        private static void EnsureCreatorsRegistered()
        {
            // Accessing MetaData triggers its static ctor which calls ValueEditor.RegisterCreator(new BasicValueEditorsCreator())
            _ = new MetaData();
        }

        #endregion

        #region ValueEditor.Create — factory tests

        [Fact]
        public void Create_EditTag_ReturnsTextValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.Text);

            // Assert
            Assert.IsType<TextValueEditor>(editor);
            Assert.Equal(EditorTags.Text, editor.Tag);
        }

        [Fact]
        public void Create_DateTimeTag_ReturnsDateTimeValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.DateTime);

            // Assert
            Assert.IsType<DateTimeValueEditor>(editor);
            Assert.Equal(EditorTags.DateTime, editor.Tag);
        }

        [Fact]
        public void Create_DateTag_ReturnsDateTimeValueEditorWithDateSubType()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.Date);

            // Assert
            var dtEditor = Assert.IsType<DateTimeValueEditor>(editor);
            Assert.Equal(DataType.Date, dtEditor.SubType);
        }

        [Fact]
        public void Create_TimeTag_ReturnsDateTimeValueEditorWithTimeSubType()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.Time);

            // Assert
            var dtEditor = Assert.IsType<DateTimeValueEditor>(editor);
            Assert.Equal(DataType.Time, dtEditor.SubType);
        }

        [Fact]
        public void Create_CustomListTag_ReturnsCustomListValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.CustomList);

            // Assert
            Assert.IsType<CustomListValueEditor>(editor);
            Assert.Equal(EditorTags.CustomList, editor.Tag);
        }

        [Fact]
        public void Create_ConstListTag_ReturnsConstListValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.ConstList);

            // Assert
            Assert.IsType<ConstListValueEditor>(editor);
            Assert.Equal(EditorTags.ConstList, editor.Tag);
        }

        [Fact]
        public void Create_ConstListBoxTag_ReturnsConstListValueEditorWithListBoxControlType()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.ConstListBox);

            // Assert
            var listEditor = Assert.IsType<ConstListValueEditor>(editor);
            Assert.Equal("LISTBOX", listEditor.ControlType);
        }

        [Fact]
        public void Create_ConstListMultyTag_ReturnsConstListValueEditorWithMultilistControlType()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.ConstListMulty);

            // Assert
            var listEditor = Assert.IsType<ConstListValueEditor>(editor);
            Assert.Equal("MULTILIST", listEditor.ControlType);
        }

        [Fact]
        public void Create_CustomTag_ReturnsCustomValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.Custom);

            // Assert
            Assert.IsType<CustomValueEditor>(editor);
        }

        [Fact]
        public void Create_FileTag_ReturnsFileValueEditor()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = ValueEditor.Create(EditorTags.File);

            // Assert
            Assert.IsType<FileValueEditor>(editor);
            Assert.Equal(EditorTags.File, editor.Tag);
        }

        [Fact]
        public void Create_UnknownTag_ReturnsCustomValueEditorWithTag()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var unknownTag = "SOMETHINGUNKNOWN";

            // Act
            var editor = ValueEditor.Create(unknownTag);

            // Assert
            var customEditor = Assert.IsType<CustomValueEditor>(editor);
            Assert.Equal(unknownTag, customEditor.Tag);
        }

        #endregion

        #region ValueEditor.RegisterCreator

        [Fact]
        public void RegisterCreator_ValidCreator_ReturnsTrue()
        {
            // Arrange
            var creator = new BasicValueEditorsCreator();

            // Act
            var result = ValueEditor.RegisterCreator(creator);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region ValueEditor constructor

        [Fact]
        public void Constructor_WithId_SetsId()
        {
            // Arrange
            var id = "myEditor";

            // Act
            var editor = new TextValueEditor(id);

            // Assert
            Assert.Equal(id, editor.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_WithNullOrEmptyId_ThrowsArgumentNullException(string id)
        {
            // Arrange / Act / Assert
            // Using ConstListValueEditor which calls base(id) -> ValueEditor(string id) that validates
            Assert.Throws<ArgumentNullException>(() => new ConstListValueEditor(id));
        }

        #endregion

        #region ValueEditorException

        [Fact]
        public void ValueEditorException_DefaultConstructor_CreatesException()
        {
            // Arrange / Act
            var ex = new ValueEditorException();

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<ValueEditorException>(ex);
        }

        [Fact]
        public void ValueEditorException_MessageConstructor_SetsMessage()
        {
            // Arrange
            var message = "Something went wrong";

            // Act
            var ex = new ValueEditorException(message);

            // Assert
            Assert.Equal(message, ex.Message);
        }

        #endregion

        #region TextValueEditor

        [Fact]
        public void TextValueEditor_PublicConstructor_SetsIdAndType()
        {
            // Arrange / Act
            var editor = new TextValueEditor("TVE01", DataType.Int32);

            // Assert
            Assert.Equal("TVE01", editor.Id);
            Assert.Equal(DataType.Int32, editor.ResultType);
            Assert.Equal(EditorTags.Text, editor.Tag);
            Assert.Equal("TVE", editor.IdBase);
        }

        [Fact]
        public void TextValueEditor_PublicConstructor_DefaultType_IsString()
        {
            // Arrange / Act
            var editor = new TextValueEditor("TVE01");

            // Assert
            Assert.Equal(DataType.String, editor.ResultType);
        }

        [Fact]
        public void TextValueEditor_DefaultValue_GetSet()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                // Act
                DefaultValue = "hello"
            };

            // Assert
            Assert.Equal("hello", editor.DefaultValue);
        }

        [Fact]
        public void TextValueEditor_DefaultText_ReturnsDefaultValue()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                DefaultValue = "hello"
            };

            // Act
            var text = editor.DefaultText;

            // Assert
            Assert.Equal("hello", text);
        }

        [Fact]
        public void TextValueEditor_DefaultText_SetterIsNoOp()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                DefaultValue = "original",

                // Act
                DefaultText = "changed"
            };

            // Assert
            Assert.Equal("original", editor.DefaultText);
        }

        [Fact]
        public void TextValueEditor_Multiline_DefaultIsFalse()
        {
            // Arrange / Act
            var editor = new TextValueEditor("TVE01");

            // Assert
            Assert.False(editor.Multiline);
        }

        [Fact]
        public void TextValueEditor_Multiline_CanBeSetToTrue()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                // Act
                Multiline = true
            };

            // Assert
            Assert.True(editor.Multiline);
        }

        [Fact]
        public void TextValueEditor_ResultType_GetSet()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                // Act
                ResultType = DataType.Float
            };

            // Assert
            Assert.Equal(DataType.Float, editor.ResultType);
        }

        [Fact]
        public void TextValueEditor_XmlDefinition_ContainsValueAndMultiline()
        {
            // Arrange
            var editor = new TextValueEditor("TVE01")
            {
                DefaultValue = "test",
                Multiline = true
            };

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("value=\"test\"", xml);
            Assert.Contains("multiline=\"True\"", xml);
            Assert.StartsWith("<Edit", xml);
        }

        [Fact]
        public async Task TextValueEditor_JsonRoundTrip_PreservesAllPropertiesAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new TextValueEditor("TVE_ROUND", DataType.Int32)
            {
                DefaultValue = "42",
                Multiline = true
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<TextValueEditor>(deserialized);
            Assert.Equal("TVE_ROUND", restored.Id);
            Assert.Equal(DataType.Int32, restored.ResultType);
            Assert.Equal("42", restored.DefaultValue);
            Assert.True(restored.Multiline);
        }

        [Fact]
        public async Task TextValueEditor_JsonRoundTrip_MultilineFalse_OmitsMultilinePropertyAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new TextValueEditor("TVE_NML", DataType.String)
            {
                DefaultValue = "abc",
                Multiline = false
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<TextValueEditor>(deserialized);
            Assert.Equal("abc", restored.DefaultValue);
            Assert.False(restored.Multiline);
        }

        #endregion

        #region DateTimeValueEditor

        [Fact]
        public void DateTimeValueEditor_DefaultConstructor_SetsDateSubTypeAndDefaultMacro()
        {
            // Arrange / Act
            EnsureCreatorsRegistered();
            var editor = ValueEditor.Create(EditorTags.DateTime) as DateTimeValueEditor;

            // Assert
            Assert.NotNull(editor);
            Assert.Equal(DataType.Date, editor.SubType);
            Assert.Equal("${{Today}}", editor.DefaultValue);
            Assert.Equal("Today", editor.DefaultText);
        }

        [Fact]
        public void DateTimeValueEditor_ConstructorWithIdAndSubType_SetsProperties()
        {
            // Arrange / Act
            var editor = new DateTimeValueEditor("DTE01", DataType.Time);

            // Assert
            Assert.Equal("DTE01", editor.Id);
            Assert.Equal(DataType.Time, editor.SubType);
            Assert.Equal(EditorTags.DateTime, editor.Tag);
        }

        [Fact]
        public void DateTimeValueEditor_ImplementsIDefaultValuesStorage()
        {
            // Arrange / Act
            var editor = new DateTimeValueEditor("DTE01", DataType.Date);

            // Assert
            Assert.IsAssignableFrom<IDefaultValuesStorage>(editor);
            Assert.NotNull(editor.DefaultValues);
        }

        [Fact]
        public void DateTimeValueEditor_SubType_SetterRecalcsDefaultValue()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.Date)
            {
                // Act
                SubType = DataType.Time
            };

            // Assert
            Assert.Equal(DataType.Time, editor.SubType);
            Assert.Equal("${{Today}}", editor.DefaultValue);
            Assert.Equal("Today", editor.DefaultText);
        }

        [Fact]
        public void DateTimeValueEditor_DefaultValue_SetWithMacro_SetsDefaultText()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.Date)
            {
                // Act
                DefaultValue = "${{Yesterday}}"
            };

            // Assert
            Assert.Equal("${{Yesterday}}", editor.DefaultValue);
            Assert.Equal("Yesterday", editor.DefaultText);
        }

        [Fact]
        public void DateTimeValueEditor_DefaultValue_SetEmpty_SetsDefaultTextEmpty()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.Date)
            {
                // Act
                DefaultValue = ""
            };

            // Assert
            Assert.Equal("", editor.DefaultValue);
            Assert.Equal(string.Empty, editor.DefaultText);
        }

        [Fact]
        public void DateTimeValueEditor_XmlDefinition_DateSubType_ContainsDATE()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.Date);

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("subType=\"DATE\"", xml);
            Assert.StartsWith("<DateTime", xml);
        }

        [Fact]
        public void DateTimeValueEditor_XmlDefinition_TimeSubType_ContainsTIME()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.Time);

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("subType=\"TIME\"", xml);
        }

        [Fact]
        public void DateTimeValueEditor_XmlDefinition_DateTimeSubType_ContainsDATETIME()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE01", DataType.DateTime);

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("subType=\"DATETIME\"", xml);
        }

        [Fact]
        public async Task DateTimeValueEditor_JsonRoundTrip_PreservesSubTypeAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new DateTimeValueEditor("DTE_RT", DataType.Time);

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<DateTimeValueEditor>(deserialized);
            Assert.Equal("DTE_RT", restored.Id);
            Assert.Equal(DataType.Time, restored.SubType);
        }

        [Fact]
        public async Task DateTimeValueEditor_JsonRoundTrip_DefaultValueResetBySubTypeAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            // SubType setter calls RecalcDefValue() which resets DefaultValue to "${{Today}}"
            // When deserializing, subType is read after dval, so dval gets overwritten.
            var original = new DateTimeValueEditor("DTE_MAC", DataType.Date)
            {
                DefaultValue = "${{Now}}"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<DateTimeValueEditor>(deserialized);
            // SubType setter resets the default value to "${{Today}}" during deserialization
            Assert.Equal("${{Today}}", restored.DefaultValue);
            Assert.Equal("Today", restored.DefaultText);
        }

        #endregion

        #region CustomValueEditor

        [Fact]
        public void CustomValueEditor_DefaultConstructor_SetsEmptyTag()
        {
            // Arrange / Act
            var editor = new CustomValueEditor();

            // Assert
            Assert.Equal("", editor.Tag);
        }

        [Fact]
        public void CustomValueEditor_TagConstructor_SetsTag()
        {
            // Arrange / Act
            var editor = new CustomValueEditor("MYTAG");

            // Assert
            Assert.Equal("MYTAG", editor.Tag);
        }

        [Fact]
        public void CustomValueEditor_DefaultValue_AlwaysReturnsEmpty()
        {
            // Arrange
            var editor = new CustomValueEditor("X")
            {
                // Act
                DefaultValue = "anything"
            };

            // Assert
            Assert.Equal("", editor.DefaultValue);
        }

        [Fact]
        public void CustomValueEditor_DefaultText_AlwaysReturnsEmpty()
        {
            // Arrange
            var editor = new CustomValueEditor("X")
            {
                // Act
                DefaultText = "anything"
            };

            // Assert
            Assert.Equal("", editor.DefaultText);
        }

        [Fact]
        public void CustomValueEditor_Data_GetSet()
        {
            // Arrange
            var editor = new CustomValueEditor("X")
            {
                // Act
                Data = "custom-data-payload"
            };

            // Assert
            Assert.Equal("custom-data-payload", editor.Data);
        }

        [Fact]
        public void CustomValueEditor_XmlDefinition_ContainsTagAndData()
        {
            // Arrange
            var editor = new CustomValueEditor("WIDGET")
            {
                Data = "param=1"
            };

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("editorType=\"WIDGET\"", xml);
            Assert.Contains("data=\"param=1\"", xml);
            Assert.StartsWith("<Label", xml);
        }

        [Fact]
        public async Task CustomValueEditor_JsonRoundTrip_PreservesDataAndTypeAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new CustomValueEditor("SPECIALWIDGET")
            {
                Data = "some-data"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<CustomValueEditor>(deserialized);
            Assert.Equal("some-data", restored.Data);
            Assert.Equal("SPECIALWIDGET", restored.Tag);
        }

        [Fact]
        public async Task CustomValueEditor_JsonRoundTrip_PreservesIdAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new CustomValueEditor("MYTYPE")
            {
                Data = "d1"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            Assert.Equal(original.Id, deserialized.Id);
        }

        #endregion

        #region CustomListValueEditor

        [Fact]
        public void CustomListValueEditor_PublicConstructor_SetsProperties()
        {
            // Arrange / Act
            var editor = new CustomListValueEditor("CLVE01", "MyList", "LISTBOX");

            // Assert
            Assert.Equal("CLVE01", editor.Id);
            Assert.Equal("MyList", editor.ListName);
            Assert.Equal("LISTBOX", editor.ControlType);
            Assert.Equal(EditorTags.CustomList, editor.Tag);
            Assert.Equal("CLVE", editor.IdBase);
        }

        [Fact]
        public void CustomListValueEditor_PublicConstructor_DefaultControlType_IsMENU()
        {
            // Arrange / Act
            var editor = new CustomListValueEditor("CLVE01", "SomeList");

            // Assert
            Assert.Equal("MENU", editor.ControlType);
        }

        [Fact]
        public void CustomListValueEditor_PublicConstructor_NullListName_SetsEmpty()
        {
            // Arrange / Act
            var editor = new CustomListValueEditor("CLVE01", null);

            // Assert
            Assert.Equal("", editor.ListName);
        }

        [Fact]
        public void CustomListValueEditor_DefaultValue_GetSet()
        {
            // Arrange
            var editor = new CustomListValueEditor("CLVE01", "L1")
            {
                // Act
                DefaultValue = "val1"
            };

            // Assert
            Assert.Equal("val1", editor.DefaultValue);
        }

        [Fact]
        public void CustomListValueEditor_DefaultText_GetSet()
        {
            // Arrange
            var editor = new CustomListValueEditor("CLVE01", "L1")
            {
                // Act
                DefaultText = "Display 1"
            };

            // Assert
            Assert.Equal("Display 1", editor.DefaultText);
        }

        [Fact]
        public void CustomListValueEditor_ResultType_GetSet()
        {
            // Arrange
            var editor = new CustomListValueEditor("CLVE01", "L1")
            {
                // Act
                ResultType = DataType.Int64
            };

            // Assert
            Assert.Equal(DataType.Int64, editor.ResultType);
        }

        [Fact]
        public void CustomListValueEditor_XmlDefinition_ContainsListNameAndControlType()
        {
            // Arrange
            var editor = new CustomListValueEditor("CLVE01", "Products", "MENU");

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("listName=\"Products\"", xml);
            Assert.Contains("controlType=\"MENU\"", xml);
            Assert.Contains("id=\"CLVE01\"", xml);
        }

        [Fact]
        public async Task CustomListValueEditor_JsonRoundTrip_PreservesListNameAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new CustomListValueEditor("CLVE_RT", "Categories", "MENU")
            {
                DefaultValue = "1"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<CustomListValueEditor>(deserialized);
            Assert.Equal("CLVE_RT", restored.Id);
            Assert.Equal("Categories", restored.ListName);
        }

        #endregion

        #region ConstListValueEditor

        [Fact]
        public void ConstListValueEditor_PublicConstructor_InitializesValues()
        {
            // Arrange / Act
            var editor = new ConstListValueEditor("CLE01");

            // Assert
            Assert.NotNull(editor.Values);
            Assert.Empty(editor.Values);
            Assert.Equal(EditorTags.ConstList, editor.Tag);
        }

        [Fact]
        public void ConstListValueEditor_DefaultValue_EmptyValues_ReturnsEmpty()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");

            // Act
            var val = editor.DefaultValue;

            // Assert
            Assert.Equal("", val);
        }

        [Fact]
        public void ConstListValueEditor_DefaultText_EmptyValues_ReturnsEmpty()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");

            // Act
            var text = editor.DefaultText;

            // Assert
            Assert.Equal("", text);
        }

        [Fact]
        public void ConstListValueEditor_DefaultValue_WithValues_ReturnsFirstId()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");
            editor.Values.Add("id1", "Text 1");
            editor.Values.Add("id2", "Text 2");

            // Act
            var val = editor.DefaultValue;

            // Assert
            Assert.Equal("id1", val);
        }

        [Fact]
        public void ConstListValueEditor_DefaultText_WithValues_ReturnsFirstText()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");
            editor.Values.Add("id1", "Text 1");
            editor.Values.Add("id2", "Text 2");

            // Act
            var text = editor.DefaultText;

            // Assert
            Assert.Equal("Text 1", text);
        }

        [Fact]
        public void ConstListValueEditor_DefaultValue_Setter_IsNoOp()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");
            editor.Values.Add("id1", "Text 1");

            // Act
            editor.DefaultValue = "something_else";

            // Assert
            Assert.Equal("id1", editor.DefaultValue);
        }

        [Fact]
        public void ConstListValueEditor_DefaultText_Setter_IsNoOp()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE_DTS");
            editor.Values.Add("id1", "Text 1");

            // Act
            editor.DefaultText = "overridden text";

            // Assert
            Assert.Equal("Text 1", editor.DefaultText);
        }

        [Fact]
        public void ConstListValueEditor_XmlDefinition_ContainsItems()
        {
            // Arrange
            var editor = new ConstListValueEditor("CLE01");
            editor.Values.Add("a", "Alpha");
            editor.Values.Add("b", "Beta");

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("<List", xml);
            Assert.Contains("value=\"a\"", xml);
            Assert.Contains("text=\"Alpha\"", xml);
            Assert.Contains("value=\"b\"", xml);
            Assert.Contains("text=\"Beta\"", xml);
        }

        [Fact]
        public async Task ConstListValueEditor_JsonRoundTrip_PreservesValuesAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new ConstListValueEditor("CLE_RT");
            original.Values.Add("1", "One");
            original.Values.Add("2", "Two");
            original.Values.Add("3", "Three");

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<ConstListValueEditor>(deserialized);
            Assert.Equal("CLE_RT", restored.Id);
            Assert.Equal(3, restored.Values.Count);
            Assert.Equal("1", restored.Values[0].Id);
            Assert.Equal("One", restored.Values[0].Text);
            Assert.Equal("2", restored.Values[1].Id);
            Assert.Equal("Two", restored.Values[1].Text);
            Assert.Equal("3", restored.Values[2].Id);
            Assert.Equal("Three", restored.Values[2].Text);
        }

        [Fact]
        public async Task ConstListValueEditor_JsonRoundTrip_EmptyValues_PreservesEmptyListAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new ConstListValueEditor("CLE_EMPTY");

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<ConstListValueEditor>(deserialized);
            Assert.Empty(restored.Values);
        }

        #endregion

        #region ConstValueList

        [Fact]
        public void ConstValueList_Add_ReturnsIndex()
        {
            // Arrange
            var list = new ConstValueList();

            // Act
            var index0 = list.Add("a", "Alpha");
            var index1 = list.Add("b", "Beta");

            // Assert
            Assert.Equal(0, index0);
            Assert.Equal(1, index1);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void ConstValueList_Add_SetsIdAndText()
        {
            // Arrange
            var list = new ConstValueList
            {
                // Act
                { "key", "value" }
            };

            // Assert
            Assert.Equal("key", list[0].Id);
            Assert.Equal("value", list[0].Text);
        }

        #endregion

        #region ListValueEditor (abstract, via ConstListValueEditor)

        [Fact]
        public void ListValueEditor_ControlType_DefaultIsMENU()
        {
            // Arrange / Act
            var editor = new ConstListValueEditor("LVE01");

            // Assert
            Assert.Equal("MENU", editor.ControlType);
        }

        [Fact]
        public void ListValueEditor_Multiselect_DefaultIsFalse()
        {
            // Arrange / Act
            var editor = new ConstListValueEditor("LVE01");

            // Assert
            Assert.False(editor.Multiselect);
        }

        [Fact]
        public void ListValueEditor_ExtraParams_DefaultIsEmpty()
        {
            // Arrange / Act
            var editor = new ConstListValueEditor("LVE01");

            // Assert
            Assert.NotNull(editor.ExtraParams);
            Assert.Empty(editor.ExtraParams);
        }

        [Fact]
        public async Task ListValueEditor_ExtraParams_JsonRoundTrip_PreservesParamsAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new ConstListValueEditor("LVE_EP");
            original.ExtraParams.Add("param1", "value1");
            original.ExtraParams.Add("param2", "value2");

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<ConstListValueEditor>(deserialized);
            Assert.Equal(2, restored.ExtraParams.Count);
            Assert.Equal("value1", restored.ExtraParams["param1"]);
            Assert.Equal("value2", restored.ExtraParams["param2"]);
        }

        [Fact]
        public async Task ListValueEditor_ExtraParams_Empty_OmitsFromJsonAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new ConstListValueEditor("LVE_NEP");

            // Act
            var json = await SerializeEditorAsync(original);

            // Assert
            Assert.DoesNotContain("extraParams", json);
        }

        #endregion

        #region FileValueEditor

        [Fact]
        public void FileValueEditor_DefaultConstructor_SetsDefaults()
        {
            // Arrange / Act
            var editor = new FileValueEditor();

            // Assert
            Assert.Equal(EditorTags.File, editor.Tag);
            Assert.Equal("*.*", editor.Accept);
        }

        [Fact]
        public void FileValueEditor_IdConstructor_SetsId()
        {
            // Arrange / Act
            var editor = new FileValueEditor("FILE01");

            // Assert
            Assert.Equal("FILE01", editor.Id);
        }

        [Fact]
        public async Task FileValueEditor_JsonRoundTrip_PreservesAcceptAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new FileValueEditor("FILE_RT")
            {
                Accept = ".jpg,.png"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<FileValueEditor>(deserialized);
            Assert.Equal("FILE_RT", restored.Id);
            Assert.Equal(".jpg,.png", restored.Accept);
        }

        #endregion

        #region ValueEditor JSON base serialization

        [Fact]
        public async Task WriteToJsonAsync_WritesTagIdResultTypeAsync()
        {
            // Arrange
            var editor = new TextValueEditor("TVE_JSON", DataType.Float)
            {
                DefaultValue = "3.14"
            };

            // Act
            var json = await SerializeEditorAsync(editor);

            // Assert
            Assert.Contains("\"tag\":\"EDIT\"", json);
            Assert.Contains("\"id\":\"TVE_JSON\"", json);
            Assert.Contains("\"dval\":\"3.14\"", json);
        }

        [Fact]
        public async Task WriteToJsonAsync_EmptyDefaultValue_OmitsDvalAsync()
        {
            // Arrange
            var editor = new TextValueEditor("TVE_NODVAL", DataType.String)
            {
                DefaultValue = ""
            };

            // Act
            var json = await SerializeEditorAsync(editor);

            // Assert
            Assert.DoesNotContain("\"dval\"", json);
        }

        [Fact]
        public async Task ReadFromJsonAsync_InvalidToken_ThrowsBadJsonFormatExceptionAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var json = "[1,2,3]"; // starts with StartArray, not StartObject

            // Act / Assert
            await Assert.ThrowsAsync<BadJsonFormatException>(async () =>
            {
                using (var sr = new StringReader(json))
                using (var reader = new JsonTextReader(sr))
                {
                    await reader.ReadAsync(); // advance to StartArray
                    await ValueEditor.ReadFromJsonAsync(reader);
                }
            });
        }

        [Fact]
        public async Task ReadFromJsonAsync_MissingTagProperty_ThrowsBadJsonFormatExceptionAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var json = "{\"id\":\"test\",\"rtype\":1}"; // no "tag" property

            // Act / Assert
            await Assert.ThrowsAsync<BadJsonFormatException>(async () =>
            {
                await DeserializeEditorAsync(json);
            });
        }

        [Fact]
        public async Task ReadContentFromJsonAsync_UnknownProperty_IsSkippedAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var json = "{\"tag\":\"EDIT\",\"id\":\"TVE01\",\"unknownField\":\"value\",\"rtype\":1}";

            // Act
            var editor = await DeserializeEditorAsync(json);

            // Assert
            Assert.Equal("TVE01", editor.Id);
            Assert.Equal(DataType.String, editor.ResultType);
        }

        #endregion

        #region ValueEditorList

        [Fact]
        public void IndexById_ExistingEditor_ReturnsIndex()
        {
            // Arrange
            var list = new ValueEditorList();
            var editor = new TextValueEditor("VE_IDX");
            list.Add(editor);

            // Act
            var index = list.IndexById("VE_IDX");

            // Assert
            Assert.Equal(0, index);
        }

        [Fact]
        public void IndexById_NonExistingEditor_ReturnsMinusOne()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var index = list.IndexById("DOES_NOT_EXIST");

            // Assert
            Assert.Equal(-1, index);
        }

        [Fact]
        public void FindById_ExistingEditor_ReturnsEditor()
        {
            // Arrange
            var list = new ValueEditorList();
            var editor = new TextValueEditor("VE_FIND");
            list.Add(editor);

            // Act
            var found = list.FindById("VE_FIND");

            // Assert
            Assert.Same(editor, found);
        }

        [Fact]
        public void FindById_NonExistingEditor_ReturnsNull()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var found = list.FindById("NONEXISTENT");

            // Assert
            Assert.Null(found);
        }

        [Fact]
        public void RemoveItem_RemovesFromSearchIndex()
        {
            // Arrange
            var list = new ValueEditorList();
            var editor = new TextValueEditor("VE_RM");
            list.Add(editor);

            // Act
            list.Remove(editor);

            // Assert
            Assert.Equal(-1, list.IndexById("VE_RM"));
        }

        [Fact]
        public void ClearItems_ClearsSearchIndex()
        {
            // Arrange
            var list = new ValueEditorList
            {
                new TextValueEditor("VE_C1"),
                new TextValueEditor("VE_C2")
            };

            // Act
            list.Clear();

            // Assert
            Assert.Equal(-1, list.IndexById("VE_C1"));
            Assert.Equal(-1, list.IndexById("VE_C2"));
            Assert.Empty(list);
        }

        [Fact]
        public void NormalizeId_CustomListValueEditor_ReturnsCLVEPrefix()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.NormalizeId("CustomList value editor");

            // Assert
            Assert.StartsWith("CLVE", result);
        }

        [Fact]
        public void NormalizeId_DateTimeValueEditor_ReturnsDTVEPrefix()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.NormalizeId("DateTime value editor");

            // Assert
            Assert.StartsWith("DTVE", result);
        }

        [Fact]
        public void NormalizeId_TextValueEditor_ReturnsTxtVEPrefix()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.NormalizeId("Text value editor");

            // Assert
            Assert.StartsWith("TxtVE", result);
        }

        [Fact]
        public void NormalizeId_SqlListValueEditor_ReturnsSLVEPrefix()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.NormalizeId("SqlList value editor");

            // Assert
            Assert.StartsWith("SLVE", result);
        }

        [Fact]
        public void NormalizeId_UnknownId_UsesIdAsBase()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.NormalizeId("SomeOther");

            // Assert
            Assert.StartsWith("SomeOther", result);
        }

        [Fact]
        public void NormalizeId_ConflictingId_IncrementsNumber()
        {
            // Arrange
            var list = new ValueEditorList();
            var existingEditor = new TextValueEditor("CLVE01");
            list.Add(existingEditor);

            // Act
            var result = list.NormalizeId("CustomList value editor");

            // Assert
            Assert.Equal("CLVE02", result);
        }

        [Fact]
        public void ConvertOldSpecialDateTimeEditor_NonCustomList_ReturnsOriginal()
        {
            // Arrange
            var list = new ValueEditorList();
            var editor = new TextValueEditor("TVE01");

            // Act
            var result = list.ConvertOldSpecialDateTimeEditor(editor);

            // Assert
            Assert.Same(editor, result);
        }

        [Fact]
        public void ConvertOldSpecialDateTimeEditor_Null_ReturnsNull()
        {
            // Arrange
            var list = new ValueEditorList();

            // Act
            var result = list.ConvertOldSpecialDateTimeEditor(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ConvertOldSpecialDateTimeEditor_CustomListWithDSDE_ReturnsMatchingEditor()
        {
            // Arrange
            var list = new ValueEditorList();
            var targetEditor = new TextValueEditor("_DSDE");
            list.Add(targetEditor);
            var oldEditor = new CustomListValueEditor("OLD01", "_DSDE")
            {
                ListName = "_DSDE"
            };

            // Act
            var result = list.ConvertOldSpecialDateTimeEditor(oldEditor);

            // Assert
            Assert.Same(targetEditor, result);
        }

        [Fact]
        public void ConvertOldSpecialDateTimeEditor_CustomListWithDSTE_ReturnsMatchingEditor()
        {
            // Arrange
            var list = new ValueEditorList();
            var targetEditor = new TextValueEditor("_DSTE");
            list.Add(targetEditor);
            var oldEditor = new CustomListValueEditor("OLD02", "_DSTE")
            {
                ListName = "_DSTE"
            };

            // Act
            var result = list.ConvertOldSpecialDateTimeEditor(oldEditor);

            // Assert
            Assert.Same(targetEditor, result);
        }

        [Fact]
        public async Task ValueEditorList_JsonRoundTrip_PreservesMultipleEditorsAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var list = new ValueEditorList();
            var textEditor = new TextValueEditor("VEL_T1", DataType.String)
            {
                DefaultValue = "hello"
            };
            var constEditor = new ConstListValueEditor("VEL_C1");
            constEditor.Values.Add("a", "Alpha");
            list.Add(textEditor);
            list.Add(constEditor);

            // Act
            var sw = new StringWriter();
            using (var writer = new JsonTextWriter(sw))
            {
                await list.WriteToJsonAsync(writer, new BitOptions(), true);
            }

            var deserialized = new ValueEditorList();
            using (var sr = new StringReader(sw.ToString()))
            using (var reader = new JsonTextReader(sr))
            {
                await reader.ReadAsync(); // advance to StartArray
                await deserialized.ReadFromJsonAsync(reader);
            }

            // Assert
            Assert.Equal(2, deserialized.Count);
            Assert.IsType<TextValueEditor>(deserialized[0]);
            Assert.IsType<ConstListValueEditor>(deserialized[1]);
            Assert.Equal("VEL_T1", deserialized[0].Id);
            Assert.Equal("VEL_C1", deserialized[1].Id);
        }

        [Fact]
        public async Task ValueEditorList_ReadFromJsonAsync_SkipsDuplicateIdsAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var json = "[{\"tag\":\"EDIT\",\"id\":\"DUP01\",\"rtype\":1,\"defval\":\"first\"},{\"tag\":\"EDIT\",\"id\":\"DUP01\",\"rtype\":1,\"defval\":\"second\"}]";

            var list = new ValueEditorList();
            using (var sr = new StringReader(json))
            using (var reader = new JsonTextReader(sr))
            {
                // Act
                await reader.ReadAsync(); // advance to StartArray
                await list.ReadFromJsonAsync(reader);
            }

            // Assert
            Assert.Single(list);
            Assert.Equal("DUP01", list[0].Id);
        }

        [Fact]
        public async Task ValueEditorList_WriteToJsonAsync_ExcludesDefaultEditors_WhenIncludeDefaultsFalseAsync()
        {
            // Arrange
            var list = new ValueEditorList();
            var defaultEditor = new TextValueEditor("DEF01")
            {
                IsDefault = true
            };
            var regularEditor = new TextValueEditor("REG01");
            list.Add(defaultEditor);
            list.Add(regularEditor);

            // Act
            var sw = new StringWriter();
            using (var writer = new JsonTextWriter(sw))
            {
                await list.WriteToJsonAsync(writer, new BitOptions(), includeDefaults: false);
            }
            var json = sw.ToString();

            // Assert
            Assert.DoesNotContain("DEF01", json);
            Assert.Contains("REG01", json);
        }

        #endregion

        #region ValueEditorStore

        [Fact]
        public void ValueEditorStore_InsertItem_SetsModelOnEditor()
        {
            // Arrange
            var model = new MetaData();
            var store = new ValueEditorStore(model);
            var editor = new TextValueEditor("STORE01");

            // Act
            store.Add(editor);

            // Assert
            Assert.Same(model, editor.Model);
        }

        [Fact]
        public void ValueEditorStore_RemoveItem_ClearsModelOnEditor()
        {
            // Arrange
            var model = new MetaData();
            var store = new ValueEditorStore(model);
            var editor = new TextValueEditor("STORE02");
            store.Add(editor);

            // Act
            store.Remove(editor);

            // Assert
            Assert.Null(editor.Model);
        }

        [Fact]
        public void ValueEditorStore_ClearItems_ClearsModelOnAllEditors()
        {
            // Arrange
            var model = new MetaData();
            var store = new ValueEditorStore(model);
            var editor1 = new TextValueEditor("STORE03");
            var editor2 = new TextValueEditor("STORE04");
            store.Add(editor1);
            store.Add(editor2);

            // Act
            store.Clear();

            // Assert
            Assert.Null(editor1.Model);
            Assert.Null(editor2.Model);
            Assert.Empty(store);
        }

        [Fact]
        public void ValueEditorStore_Model_MatchesConstructorArg()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var store = new ValueEditorStore(model);

            // Assert
            Assert.Same(model, store.Model);
        }

        #endregion

        #region ValueEditor.CheckInModel

        [Fact]
        public void CheckInModel_EditorNotInModel_AddsToEditors()
        {
            // Arrange
            var model = new MetaData();
            var editor = new TextValueEditor("CHECK01");

            // Act
            editor.CheckInModel(model);

            // Assert
            Assert.Equal(0, model.Editors.IndexById("CHECK01"));
            Assert.Same(editor, model.Editors.FindById("CHECK01"));
        }

        [Fact]
        public void CheckInModel_EditorAlreadyInModel_DoesNotAddDuplicate()
        {
            // Arrange
            var model = new MetaData();
            var editor = new TextValueEditor("CHECK02");
            editor.CheckInModel(model);
            var initialCount = model.Editors.Count;

            // Act
            editor.CheckInModel(model);

            // Assert
            Assert.Equal(initialCount, model.Editors.Count);
        }

        #endregion

        #region BasicValueEditorsCreator — comprehensive

        [Theory]
        [InlineData(EditorTags.Text, typeof(TextValueEditor))]
        [InlineData(EditorTags.DateTime, typeof(DateTimeValueEditor))]
        [InlineData(EditorTags.Date, typeof(DateTimeValueEditor))]
        [InlineData(EditorTags.Time, typeof(DateTimeValueEditor))]
        [InlineData(EditorTags.CustomList, typeof(CustomListValueEditor))]
        [InlineData(EditorTags.ConstList, typeof(ConstListValueEditor))]
        [InlineData(EditorTags.ConstListBox, typeof(ConstListValueEditor))]
        [InlineData(EditorTags.ConstListMulty, typeof(ConstListValueEditor))]
        [InlineData(EditorTags.Custom, typeof(CustomValueEditor))]
        [InlineData(EditorTags.File, typeof(FileValueEditor))]
        public void BasicValueEditorsCreator_Create_RecognizedTag_ReturnsCorrectType(string tag, Type expectedType)
        {
            // Arrange
            var creator = new BasicValueEditorsCreator();

            // Act
            var editor = creator.Create(tag);

            // Assert
            Assert.NotNull(editor);
            Assert.IsType(expectedType, editor);
        }

        [Fact]
        public void BasicValueEditorsCreator_Create_UnrecognizedTag_ReturnsNull()
        {
            // Arrange
            var creator = new BasicValueEditorsCreator();

            // Act
            var editor = creator.Create("NONEXISTENT_TAG");

            // Assert
            Assert.Null(editor);
        }

        #endregion

        #region EditorTags constants

        [Fact]
        public void EditorTags_AllConstantsHaveExpectedValues()
        {
            // Arrange / Act / Assert
            Assert.Equal("EDIT", EditorTags.Text);
            Assert.Equal("DATETIME", EditorTags.DateTime);
            Assert.Equal("DATE", EditorTags.Date);
            Assert.Equal("TIME", EditorTags.Time);
            Assert.Equal("CUSTOMLIST", EditorTags.CustomList);
            Assert.Equal("LIST", EditorTags.ConstList);
            Assert.Equal("LISTBOX", EditorTags.ConstListBox);
            Assert.Equal("MULTILIST", EditorTags.ConstListMulty);
            Assert.Equal("FILE", EditorTags.File);
            Assert.Equal("CUSTOM", EditorTags.Custom);
        }

        #endregion

        #region Complex JSON roundtrip scenarios

        [Fact]
        public async Task JsonRoundTrip_AllEditorTypes_PreservedCorrectlyAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();

            var textEditor = new TextValueEditor("RT_TEXT", DataType.Int32)
            {
                DefaultValue = "99",
                Multiline = true
            };

            var dtEditor = new DateTimeValueEditor("RT_DT", DataType.Time);

            var customEditor = new CustomValueEditor("MYTYPE")
            {
                Data = "payload"
            };

            var clEditor = new CustomListValueEditor("RT_CL", "Colors", "MENU")
            {
                DefaultValue = "red"
            };

            var constEditor = new ConstListValueEditor("RT_CONST");
            constEditor.Values.Add("m", "Male");
            constEditor.Values.Add("f", "Female");
            constEditor.ExtraParams.Add("filter", "active");

            var fileEditor = new FileValueEditor("RT_FILE")
            {
                Accept = ".pdf"
            };

            var editors = new ValueEditor[] { textEditor, dtEditor, customEditor, clEditor, constEditor, fileEditor };

            foreach (var original in editors)
            {
                // Act
                var json = await SerializeEditorAsync(original);
                var restored = await DeserializeEditorAsync(json);

                // Assert
                Assert.Equal(original.Id, restored.Id);
                Assert.Equal(original.Tag, restored.Tag);
            }
        }

        [Fact]
        public async Task JsonRoundTrip_TextValueEditor_ResultTypePreservedAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new TextValueEditor("TVE_RTYPE", DataType.Currency)
            {
                DefaultValue = "100.50"
            };

            // Act
            var json = await SerializeEditorAsync(original);
            var restored = await DeserializeEditorAsync(json);

            // Assert
            var textRestored = Assert.IsType<TextValueEditor>(restored);
            Assert.Equal(DataType.Currency, textRestored.ResultType);
            Assert.Equal("100.50", textRestored.DefaultValue);
        }

        [Fact]
        public async Task JsonRoundTrip_DateTimeValueEditor_DateSubType_PreservedAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new DateTimeValueEditor("DTE_DATE", DataType.Date);

            // Act
            var json = await SerializeEditorAsync(original);
            var restored = await DeserializeEditorAsync(json);

            // Assert
            var dtRestored = Assert.IsType<DateTimeValueEditor>(restored);
            Assert.Equal(DataType.Date, dtRestored.SubType);
            // SubType setter resets default to "${{Today}}" during deserialization
            Assert.Equal("${{Today}}", dtRestored.DefaultValue);
        }

        [Fact]
        public async Task JsonRoundTrip_CustomListValueEditor_ExtraParams_PreservedAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new CustomListValueEditor("CLVE_EP", "Regions", "LISTBOX");
            original.ExtraParams.Add("scope", "global");

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<CustomListValueEditor>(deserialized);
            Assert.Equal("Regions", restored.ListName);
            Assert.Single(restored.ExtraParams);
            Assert.Equal("global", restored.ExtraParams["scope"]);
        }

        #endregion

        #region ValueEditor.IsDefault property

        [Fact]
        public void IsDefault_DefaultIsFalse()
        {
            // Arrange / Act
            var editor = new TextValueEditor("TEST_ISDEF");

            // Assert
            Assert.False(editor.IsDefault);
        }

        [Fact]
        public void IsDefault_CanBeSetToTrue()
        {
            // Arrange
            var editor = new TextValueEditor("TEST_ISDEF2")
            {
                // Act
                IsDefault = true
            };

            // Assert
            Assert.True(editor.IsDefault);
        }

        #endregion

        #region ValueEditor base properties

        [Fact]
        public void STypeCaption_ReturnsEmptyString()
        {
            // Arrange / Act
            var caption = ValueEditor.STypeCaption;

            // Assert
            Assert.Equal("", caption);
        }

        [Fact]
        public void TextValueEditor_STypeCaption_ReturnsExpectedString()
        {
            // Arrange / Act
            var caption = TextValueEditor.STypeCaption;

            // Assert
            Assert.Equal("Text value editor", caption);
        }

        [Fact]
        public void DateTimeValueEditor_STypeCaption_ReturnsExpectedString()
        {
            // Arrange / Act
            var caption = DateTimeValueEditor.STypeCaption;

            // Assert
            Assert.Equal("Date time value editor", caption);
        }

        [Fact]
        public void CustomValueEditor_STypeCaption_ReturnsExpectedString()
        {
            // Arrange / Act
            var caption = CustomValueEditor.STypeCaption;

            // Assert
            Assert.Equal("Custom editor", caption);
        }

        [Fact]
        public void CustomListValueEditor_STypeCaption_ReturnsExpectedString()
        {
            // Arrange / Act
            var caption = CustomListValueEditor.STypeCaption;

            // Assert
            Assert.Equal("Custom list value editor", caption);
        }

        [Fact]
        public void ConstListValueEditor_STypeCaption_ReturnsExpectedString()
        {
            // Arrange / Act
            var caption = ConstListValueEditor.STypeCaption;

            // Assert
            Assert.Equal("Constant list value editor", caption);
        }

        #endregion

        #region Id property

        [Fact]
        public void Id_CanBeChanged()
        {
            // Arrange
            var editor = new TextValueEditor("ORIGINAL")
            {
                // Act
                Id = "CHANGED"
            };

            // Assert
            Assert.Equal("CHANGED", editor.Id);
        }

        #endregion

        #region ResultType base class default

        [Fact]
        public void ResultType_BaseDefault_IsUnknown()
        {
            // Arrange
            EnsureCreatorsRegistered();

            // Act
            var editor = new CustomValueEditor("X");

            // Assert
            Assert.Equal(DataType.Unknown, editor.ResultType);
        }

        #endregion

        #region XmlDefinition base class default

        [Fact]
        public void XmlDefinition_FileValueEditor_ReturnsEmptyByDefault()
        {
            // Arrange - FileValueEditor does not override XmlDefinition,
            // so it falls back to the base class default
            var editor = new FileValueEditor("FVE01");

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Equal("", xml);
        }

        #endregion

        #region CustomListValueEditor Multiselect in XmlDefinition

        [Fact]
        public void CustomListValueEditor_XmlDefinition_ContainsMultiselect()
        {
            // Arrange
            var editor = new CustomListValueEditor("CLVE_MS", "Items", "MENU")
            {
                Multiselect = true
            };

            // Act
            var xml = editor.XmlDefinition;

            // Assert
            Assert.Contains("multiselect=\"True\"", xml);
        }

        [Fact]
        public async Task FileValueEditor_MetaDataJsonRoundTrip_PreservesAcceptPropertyAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var model = new MetaData();
            var fileEditor = new FileValueEditor("FVE_META")
            {
                Accept = ".pdf,.docx"
            };
            model.Editors.Add(fileEditor);

            // Act
            var json = model.SaveToJsonString();
            var restored = new MetaData();
            restored.LoadFromJsonString(json);

            // Assert
            var restoredEditor = restored.Editors.FindById("FVE_META");
            Assert.NotNull(restoredEditor);
            var restoredFile = Assert.IsType<FileValueEditor>(restoredEditor);
            Assert.Equal(".pdf,.docx", restoredFile.Accept);
        }

        [Fact]
        public async Task FileValueEditor_JsonRoundTrip_DefaultAccept_PreservedAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            var original = new FileValueEditor("FILE_DEFACC");

            // Act
            var json = await SerializeEditorAsync(original);
            var deserialized = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<FileValueEditor>(deserialized);
            Assert.Equal("*.*", restored.Accept);
        }

        #endregion

        #region ConstValueItem

        [Fact]
        public void ConstValueItem_Properties_GetSet()
        {
            // Arrange
            var item = new ConstValueItem
            {
                // Act
                Id = "testId",
                Text = "testText"
            };

            // Assert
            Assert.Equal("testId", item.Id);
            Assert.Equal("testText", item.Text);
        }

        #endregion

        #region ConstListValueEditor JSON edge cases

        [Fact]
        public async Task ConstListValueEditor_JsonRoundTrip_ValueItemWithUnknownProperty_SkipsItAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            // JSON with an extra unknown property "extra" inside a value item object
            var json = "{\"tag\":\"LIST\",\"id\":\"CLE_SKIP\",\"rtype\":0,\"values\":[{\"id\":\"1\",\"text\":\"One\",\"extra\":\"ignored\"},{\"id\":\"2\",\"text\":\"Two\"}]}";

            // Act
            var editor = await DeserializeEditorAsync(json);

            // Assert
            var restored = Assert.IsType<ConstListValueEditor>(editor);
            Assert.Equal(2, restored.Values.Count);
            Assert.Equal("1", restored.Values[0].Id);
            Assert.Equal("One", restored.Values[0].Text);
            Assert.Equal("2", restored.Values[1].Id);
            Assert.Equal("Two", restored.Values[1].Text);
        }

        [Fact]
        public async Task ConstListValueEditor_ReadFromJson_ValuesNotArray_ThrowsBadJsonFormatExceptionAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            // "values" is a string instead of an array
            var json = "{\"tag\":\"LIST\",\"id\":\"CLE_BAD\",\"rtype\":0,\"values\":\"not-an-array\"}";

            // Act / Assert
            await Assert.ThrowsAsync<BadJsonFormatException>(async () =>
            {
                await DeserializeEditorAsync(json);
            });
        }

        [Fact]
        public async Task ConstListValueEditor_ReadFromJson_ValueItemNotObject_ThrowsBadJsonFormatExceptionAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            // Array contains a primitive instead of an object
            var json = "{\"tag\":\"LIST\",\"id\":\"CLE_BAD2\",\"rtype\":0,\"values\":[\"not-an-object\"]}";

            // Act / Assert
            await Assert.ThrowsAsync<BadJsonFormatException>(async () =>
            {
                await DeserializeEditorAsync(json);
            });
        }

        #endregion

        #region ListValueEditor ExtraParams JSON edge cases

        [Fact]
        public async Task ListValueEditor_ReadFromJson_ExtraParamsNotObject_ThrowsBadJsonFormatExceptionAsync()
        {
            // Arrange
            EnsureCreatorsRegistered();
            // "extraParams" is a string instead of an object
            var json = "{\"tag\":\"LIST\",\"id\":\"LVE_BAD_EP\",\"rtype\":0,\"values\":[],\"extraParams\":\"not-an-object\"}";

            // Act / Assert
            await Assert.ThrowsAsync<BadJsonFormatException>(async () =>
            {
                await DeserializeEditorAsync(json);
            });
        }

        #endregion

        #region DateTimeValueEditor additional coverage

        [Fact]
        public void DateTimeValueEditor_DefaultValue_SetNonMacroDateString_SetsDefaultTextToUserFormat()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE_PLAIN", DataType.Date)
            {
                // Act
                DefaultValue = "2024-06-15"
            };

            // Assert
            Assert.Equal("2024-06-15", editor.DefaultValue);
            // DefaultText should be the user-formatted date (not a macro name and not empty)
            Assert.NotEqual(string.Empty, editor.DefaultText);
            Assert.NotEqual("2024-06-15", editor.DefaultText);
        }

        [Fact]
        public void DateTimeValueEditor_DefaultValue_SetNonMacroTimeString_SetsDefaultTextToUserFormat()
        {
            // Arrange
            var editor = new DateTimeValueEditor("DTE_TIME", DataType.Time)
            {
                // Act
                DefaultValue = "14:30:00"
            };

            // Assert
            Assert.Equal("14:30:00", editor.DefaultValue);
            Assert.NotEqual(string.Empty, editor.DefaultText);
        }

        #endregion
    }
}
