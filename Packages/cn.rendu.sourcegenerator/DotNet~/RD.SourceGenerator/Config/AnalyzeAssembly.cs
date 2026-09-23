using System.IO;

namespace RD
{
    public static class AnalyzeAssembly
    {
        private const string Core = "RD.Core";
        private const string Model = "RD.Model";
        private const string Hotfix = "RD.Hotfix";
        private const string ModelView = "RD.ModelView";
        private const string HotfixView = "RD.HotfixView";

        public static readonly string[] AllHotfix =
        [
            Hotfix, HotfixView
        ];

        public static readonly string[] AllModel =
        [
            Model, ModelView
        ];

        public static readonly string[] AllModelHotfix =
        [
            Model, Hotfix, ModelView, HotfixView
        ];
        
        public static readonly string[] All =
        [
            Core, Model, Hotfix, ModelView, HotfixView
        ];
        
        public static readonly string[] AllLogicModel =
        [
            Model
        ];
    }
    
}