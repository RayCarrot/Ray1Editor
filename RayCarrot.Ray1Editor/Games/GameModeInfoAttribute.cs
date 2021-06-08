using System;

namespace RayCarrot.Ray1Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GameModeInfoAttribute : Attribute
    {
        public GameModeInfoAttribute(string displayName, Type managerType, GameModePathType pathType)
        {
            DisplayName = displayName;
            ManagerType = managerType;
            PathType = pathType;
        }

        public string DisplayName { get; }
        public Type ManagerType { get; }
        public GameModePathType PathType { get; }
    }
}