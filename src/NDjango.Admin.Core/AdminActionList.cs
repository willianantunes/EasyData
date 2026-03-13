using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDjango.Admin
{
    public class AdminActionList<TPk>
    {
        private readonly List<AdminActionRegistration<TPk>> _actions = new();

        public AdminActionList<TPk> Add(string name, string description,
            Func<IServiceProvider, IReadOnlyList<TPk>, Task<AdminActionResult>> handler,
            bool allowEmptySelection = false)
        {
            ArgumentNullException.ThrowIfNull(handler);
            _actions.Add(new AdminActionRegistration<TPk> { Name = name, Description = description, Handler = handler, AllowEmptySelection = allowEmptySelection });
            return this;
        }

        public IReadOnlyList<AdminActionRegistration<TPk>> Actions => _actions;
    }

    public class AdminActionRegistration<TPk>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AllowEmptySelection { get; set; }
        public Func<IServiceProvider, IReadOnlyList<TPk>, Task<AdminActionResult>> Handler { get; set; }
    }
}
