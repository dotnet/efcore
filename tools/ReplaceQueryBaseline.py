import os

repository_path = os.getcwd()
if repository_path.endswith("tools"):
    repository_path = repository_path[:-6]

repository_path = repository_path + os.sep
query_base_line_path = repository_path + "QueryBaseline.txt"

map = { }
for a in open(query_base_line_path).read().split("\n\n--------------------"):
    i = -1
    for b in a.split("\n"):
        if (i == -1):
            lines = b.split("\n")
            number_string = lines[0].split(" : ")[-1]
            if (number_string == ''):
                continue
            test_name_parts = lines[0].split(" : ")[0].split(".")
            if test_name_parts[2] == "Query" or test_name_parts[2] == "BulkUpdates" or test_name_parts[2] == "Update":
                test_file_identifier = test_name_parts[2] + "\\" + test_name_parts[3] + ".cs"
            else:
                test_file_identifier = test_name_parts[2] + ".cs"
            test_provider = None
            if (test_file_identifier.find("SqlServer") != -1):
                test_provider = "SqlServer"
            elif (test_file_identifier.find("Sqlite") != -1):
                test_provider = "Sqlite"
            elif (test_file_identifier.find("Cosmos") != -1):
                test_provider = "Cosmos"
            
            if test_provider is None:
                continue
            if test_provider not in map:
                map[test_provider] = {}
            if test_file_identifier not in map[test_provider]:
                map[test_provider][test_file_identifier] = {}
            test_line_number = int(number_string)
            map[test_provider][test_file_identifier][test_line_number] = []
            i = i + 1
            continue
        i = i + 1
        map[test_provider][test_file_identifier][test_line_number].append(b)


for provider in map.keys():
    test_directory = repository_path + "test\\EFCore." + provider + ".FunctionalTests\\"
    for f in map[provider].keys():
        test_file_path = test_directory + f
        test_file_lines = open(test_file_path).readlines()
        keys = list(map[provider][f].keys())
        keys.sort()
        offset = 0
        for k in keys:
            index = k - 1 + offset
            while True:
                if (test_file_lines[index].strip() == "}"):
                    new_lines = map[provider][f][k][:-1]
                    new_lines.reverse()
                    for l in new_lines:
                        test_file_lines.insert(index, l + "\n")
                        offset = offset + 1
                    break
                offset = offset - 1
                del test_file_lines[index]
        
        open(test_file_path, "w+").writelines(test_file_lines)

open(query_base_line_path, "w").truncate()