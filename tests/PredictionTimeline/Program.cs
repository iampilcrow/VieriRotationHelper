using WrathCombo;

var checks = 0;
void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }

var replacements = new[]
{
    new TraitReplacementMap.Replacement(2866, 7411, 3),
    new TraitReplacementMap.Replacement(2868, 7412, 3),
    new TraitReplacementMap.Replacement(2873, 7413, 3),
    new TraitReplacementMap.Replacement(121, 132, 3),
    new TraitReplacementMap.Replacement(132, 16532, 3),
    new TraitReplacementMap.Replacement(34606, 34608, 1),
    new TraitReplacementMap.Replacement(34608, 34610, 2),
};
var map = TraitReplacementMap.Build(replacements);

Check(map[7411] == 2866, "MCH trait-upgraded starter maps to its recorded combo action");
Check(map[7412] == 2868, "MCH trait-upgraded second step maps to its recorded combo action");
Check(map[7413] == 2873, "MCH trait-upgraded finisher maps to its recorded combo action");
Check(map[132] == 121, "first trait upgrade maps to the original combo action");
Check(map[16532] == 121, "chained trait upgrades collapse to the original combo action");
Check(!map.ContainsKey(34608), "temporary replacement types are not normalized");
Check(!map.ContainsKey(34610), "dynamic combo steps remain distinct");
Check(!map.ContainsKey(36963), "unrelated actions remain untouched");

Console.WriteLine($"{checks} prediction timeline checks passed.");
