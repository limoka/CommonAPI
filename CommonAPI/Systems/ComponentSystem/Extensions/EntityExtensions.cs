namespace CommonAPI.Systems.Extensions
{
    public static class EntityExtensions
    {
        public static int GetCustomId(this EntityData entity)
        {
            return entity.customId;
        }
        
        public static int GetCustomType(this EntityData entity)
        {
            return entity.customType;
        }
    }
}