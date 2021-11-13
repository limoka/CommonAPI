using Mono.Cecil;

namespace CommonAPI {
    
// Source code is taken from R2API: https://github.com/risk-of-thunder/R2API/tree/master

    public static class CecilExtension {

        internal static bool IsSubTypeOf(this TypeDefinition typeDefinition, string typeFullName) {
            if (typeDefinition.FullName == typeFullName) {
                return true;
            }

            var typeDefBaseType = typeDefinition.BaseType?.Resolve();
            while (typeDefBaseType != null) {
                if (typeDefBaseType.FullName == typeFullName) {
                    return true;
                }

                typeDefBaseType = typeDefBaseType.BaseType?.Resolve();
            }

            return false;
        }
    }
}
