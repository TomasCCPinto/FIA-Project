using UnityEngine;
using UnityEditor;
using System.Linq;

public class ExportPackage
{
    [MenuItem("Export/MyExport")]
    static void export()
    {
        var assetsPaths = AssetDatabase.GetAllAssetPaths().ToList();
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/ElitismSolution.cs");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/ElitismSolution.cs.meta");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/SinglePointCrossoverSolution.cs");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/SolutionsSinglePointCrossoverSolution.cs.meta");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/SinglePointMutationSolution.cs");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/SinglePointMutationSolution.cs.meta");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/TournamentSolution.cs.meta");
        assetsPaths.Remove("Assets/EvolvingCars/TP2/Meta1/Solutions/TournamentSolution.cs.meta");
        Debug.Log(assetsPaths.IndexOf("Assets/EvolvingCars/TP2/Meta1/Solutions"));
        AssetDatabase.ExportPackage(assetsPaths.ToArray(), "EvolvingCars.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets); ;
    }
}
