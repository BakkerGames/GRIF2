# GROD2 Syntax

List<string> GetLevelKeys()
void AddLevel(string levelKey)
void AddLevel(string levelKey, string parentLevelKey)
void SetLevelParent(string levelKey, string? parentLevelKey)
void RemoveLevel(string levelKey)

string? Get(string levelKey, string key)
string? GetRecursive(string levelKey, string key)

void Set(string levelKey, string key, string? text)

void Remove(string levelKey, string key)

void Clear(string levelKey)

List<string> GetKeys(string levelKey)
List<string> GetKeysRecursive(string levelKey)

List<Item> GetItems(string levelKey)
List<Item> GetItemsRecursive(string levelKey)

void AddRange(string levelKey, List<Item> items)
