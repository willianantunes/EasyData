namespace NDjango.Admin
{
    public class AdminActionResult
    {
        public string Message { get; set; } = "";
        public AdminActionMessageLevel Level { get; set; } = AdminActionMessageLevel.Success;
        public static AdminActionResult Success(string message) => new() { Message = message, Level = AdminActionMessageLevel.Success };
        public static AdminActionResult Error(string message) => new() { Message = message, Level = AdminActionMessageLevel.Error };
    }

    public enum AdminActionMessageLevel { Success, Error }

    public class AdminActionDescriptor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AllowEmptySelection { get; set; }
    }
}
