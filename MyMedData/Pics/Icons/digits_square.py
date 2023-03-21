n = int(input())
max_num = (n + 1) // 2
max_length = len(str(max_num))
for i in range(0, n // 2 + n % 2):
    num = 1
    length = len(str(num))
    for j in range(i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
        num += 1
        length = len(str(num))
    for j in range(n - 2 * i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
    num -= 1
    length = len(str(num))
    for j in range(i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
        num -= 1
        length = len(str(num))
    print()
for i in range(n // 2 - 1, -1, -1):
    num = 1
    length = len(str(num))
    for j in range(i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
        num += 1
        length = len(str(num))
    for j in range(n - 2 * i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
    num -= 1
    length = len(str(num))
    for j in range(i):
        print(f"{' ' * (max_length - length)}{num}", end=" ")
        num -= 1
        length = len(str(num))
    print()
        
    
