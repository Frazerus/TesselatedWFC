using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Mathematics.OutlierDetection;

namespace Benchmarks;

public enum Type
{
    Grass,
    Normal,
    Basic
}

[RPlotExporter]
[Outliers(OutlierMode.DontRemove)]
[MemoryDiagnoser]
public class Benchmark
{
    private int[] normal10ent = new[]
    {
        328, 258, 156, 1137, 948, 1008, 966, 661, 312, 866, 190, 274, 275, 139, 1152, 716, 337, 535, 574, 1089, 774, 1247, 1121, 1292, 1294, 903, 475, 441, 453, 655, 864, 143, 882, 643, 1053, 636, 543, 1052, 80, 1264
    };

    private int[] normal10mrv = new[]
    {
        1127, 531, 538, 1019, 272, 1002, 1062, 1339, 1139, 871, 1103, 851, 523, 756, 769, 85, 41, 202, 296, 75, 395,
        362, 150, 712, 556, 870, 866, 1286, 923, 275, 478, 276, 21, 452, 215, 1153, 235, 840, 669, 292
    };

    private int[] normal20mrv = new[] {307, 1170, 848, 503, 593, 413, 1163, 454, 501, 566, 535, 361, 744, 8, 157, 315, 724, 1234, 493, 696, 843, 637, 1345, 464, 701, 264, 128, 955, 569, 464, 118, 742, 708, 1354, 35, 974, 535, 1260, 907, 830 };
    private int[] normal20ent = new[] {284, 1276, 291, 139, 77, 817, 1053, 539, 732, 558, 71, 1290, 427, 1306, 286, 706, 341, 69, 1091, 1134, 1010, 641, 1109, 152, 601, 1240, 338, 1168, 1, 715, 153, 581, 1178, 962, 1003, 223, 662, 1302, 83, 602 };
    private int[] normal30mrv = new[] { 614, 650, 203, 30, 1050, 1110, 991, 1216, 116, 468, 650, 1149, 405, 1250, 618, 192, 13, 958, 730, 607, 902, 1080, 1050, 1180, 238, 1072, 972, 1303, 1286, 422, 605, 193, 897, 565, 1042, 1286, 786, 902, 1155, 819};
    private int[] normal30ent = new[] {1045, 1233, 1185, 696, 1317, 1273, 1079, 682, 800, 346, 338, 903, 941, 696, 552, 1317, 714, 360, 1084, 800, 757, 111, 492, 317, 1022, 1293, 461, 1079, 176, 98, 1293, 1205, 1040, 1140, 561, 340, 821, 471, 179, 243 };

    private int[] grass10mrv = new[] {225, 837, 12, 195, 125, 986, 367, 223, 749, 903, 993, 553, 1197, 861, 765, 789, 319, 266, 1180, 1127, 293, 1281, 383, 1241, 29, 1189, 336, 1028, 74, 76, 249, 431, 490, 1171, 153, 1304, 1169, 1251, 1299, 376 };
    private int[] grass10ent = new[] { 1293, 666, 31, 1245, 850, 379, 396, 1214, 65, 550, 1223, 638, 1008, 753, 1275, 1343, 1223, 1041, 338, 1209, 46, 455, 644, 578, 746, 774, 533, 780, 715, 1037, 1047, 1241, 1086, 538, 173, 219, 668, 57, 546, 1056};
    private int[] grass20ent = new[] {1207, 1279, 317, 814, 1143, 1254, 278, 1015, 36, 284, 506, 1342, 595, 699, 910, 1275, 436, 38, 428, 1013, 320, 513, 679, 1150, 1016, 462, 701, 1234, 244, 356, 655, 924, 24, 659, 553, 23, 214, 148, 169, 257 };
    private int[] grass20mrv = new[] {732, 1098, 686, 25, 671, 1318, 194, 30, 702, 281, 674, 832, 1013, 492, 297, 121, 811, 221, 949, 889, 391, 80, 121, 963, 1127, 1213, 659, 977, 483, 1375, 1365, 887, 1183, 1307, 564, 1354, 672, 163, 845, 875 };
    private int[] grass30mrv = new[] {1047, 415, 748, 1271, 783, 791, 820, 615, 83, 1250, 711, 241, 92, 61, 841, 132, 850, 431, 1253, 1355, 517, 1142, 906, 469, 190, 251, 836, 788, 828, 731, 781, 905, 85, 700, 897, 402, 885, 106, 471, 363 };
    private int[] grass30ent = new[] {1064, 1052, 1215, 475, 56, 1312, 190, 628, 96, 731, 291, 479, 790, 131, 1219, 1141, 132, 737, 1229, 189, 314, 704, 592, 600, 1084, 311, 258, 439, 1016, 1087, 612, 460, 958, 101, 600, 303, 593, 20, 397, 1192 };

    private int[] basic10mrv = new[] {705, 795, 320, 954, 523, 1060, 1117, 713, 799, 743, 201, 1286, 403, 1070, 231, 20, 1151, 1080, 640, 910, 247, 620, 1087, 345, 218, 412, 22, 1079, 363, 1114, 37, 787, 620, 311, 1179, 684, 1316, 677, 546, 1311 };
    private int[] basic10ent = new[] {182, 709, 438, 833, 1171, 841, 943, 263, 131, 1099, 8, 229, 63, 717, 614, 107, 322, 1257, 456, 1039, 834, 365, 520, 307, 541, 784, 1158, 620, 1181, 1361, 484, 1207, 223, 351, 718, 1165, 253, 1092, 893, 1162 };
    private int[] basic20mrv = new[] { 287, 397, 390, 465, 1151, 606, 1107, 1065, 948, 1348, 143, 481, 895, 415, 248, 485, 1271, 223, 1188, 217, 366, 187, 238, 808, 283, 406, 1122, 594, 1072, 904, 139, 292, 1222, 38, 196, 635, 834, 485, 205, 1353};
    private int[] basic20ent = new[] {614, 1349, 1096, 718, 1141, 1318, 1340, 914, 1317, 881, 1166, 893, 618, 730, 816, 224, 911, 406, 660, 643, 291, 1039, 238, 400, 527, 818, 196, 499, 921, 299, 175, 830, 1124, 1194, 569, 1110, 763, 857, 918, 1223 };
    private int[] basic30mrv = new[] {292, 1249, 1036, 858, 1109, 873, 769, 438, 1003, 61, 1359, 489, 491, 468, 1187, 460, 122, 788, 1002, 715, 632, 207, 911, 931, 967, 1165, 1208, 51, 693, 702, 247, 447, 375, 110, 639, 620, 1275, 1168, 1110, 319 };
    private int[] basic30ent = new[] {734, 349, 75, 861, 99, 83, 890, 1349, 698, 751, 1323, 557, 816, 636, 225, 279, 359, 1049, 832, 966, 619, 110, 540, 1109, 631, 6, 1148, 404, 46, 317, 794, 335, 1182, 1184, 781, 1370, 1308, 1280, 917, 23 };

    private const string CPathToXmlFile = @"..\..\..\..\..\Assets";

    [Params(Heuristic.Entropy, Heuristic.MRV)]
    public Heuristic Heuristic { get; set; }

    [Params(30, 20, 10)] public int Size { get; set; }

    [Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39)]
    public int s { get; set; }

    public int Seed(Type info)
    {
        return (Size, Heuristic, info) switch
        {
            (10, Heuristic.Entropy, Type.Grass) => grass10ent[s],
            (20, Heuristic.Entropy, Type.Grass) => grass20ent[s],
            (30, Heuristic.Entropy, Type.Grass) => grass30ent[s],
            (10, Heuristic.MRV, Type.Grass) => grass10mrv[s],
            (20, Heuristic.MRV, Type.Grass) => grass20mrv[s],
            (30, Heuristic.MRV, Type.Grass) => grass30mrv[s],
            (10, Heuristic.Entropy, Type.Basic) => basic10ent[s],
            (20, Heuristic.Entropy, Type.Basic) => basic20ent[s],
            (30, Heuristic.Entropy, Type.Basic) => basic30ent[s],
            (10, Heuristic.MRV, Type.Basic) => basic10mrv[s],
            (20, Heuristic.MRV, Type.Basic) => basic20mrv[s],
            (30, Heuristic.MRV, Type.Basic) => basic30mrv[s],
            (10, Heuristic.Entropy, Type.Normal) => normal10ent[s],
            (20, Heuristic.Entropy, Type.Normal) => normal20ent[s],
            (30, Heuristic.Entropy, Type.Normal) => normal30ent[s],
            (10, Heuristic.MRV, Type.Normal) => normal10mrv[s],
            (20, Heuristic.MRV, Type.Normal) => normal20mrv[s],
            (30, Heuristic.MRV, Type.Normal) => normal30mrv[s],
            _ => -1
        };
    }

    [Benchmark]
    public void OriginalModel()
    {
        var model = new SimpleTiledModel("standard", null, Size, Size, false, Heuristic, "standard",
            CPathToXmlFile);
        model.Run(Seed(Type.Basic), 10_000);
    }

    [Benchmark]
    public void OctagonTessellationModel_NormalSquares()
    {
        var model = new SimpleOctagonTessellationModel("octagon_standardImitation_NormalSquares", Size, Size, false,
            Heuristic, 2, CPathToXmlFile);
        model.Run(Seed(Type.Normal), 10_000);
    }

    [Benchmark]
    public void OctagonTessellationModel_GrassSquares()
    {
        var model = new SimpleOctagonTessellationModel("octagon_standardImitation_GrassOnlySquares", Size, Size,
            false,
            Heuristic, 2, CPathToXmlFile);
        model.Run(Seed(Type.Grass), 10_000);
    }
}




public class Program
{
    public static void Main()
    {

        var config =
            DefaultConfig.Instance.AddJob(Job.MediumRun.WithLaunchCount(1)
                .WithToolchain(InProcessNoEmitToolchain.Instance));
        //BenchmarkRunner.Run<Benchmark.NormalSquares>(config);
        BenchmarkRunner.Run(typeof(Benchmark).Assembly, config);
    }
}