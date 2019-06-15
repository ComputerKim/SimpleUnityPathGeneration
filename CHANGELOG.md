### 1.1 - 2019-06-15
## Added
- Github
- Will now only check a point once, this will prevent it from making mistakes

## Changes
- When reaching max loops the path will go down instead of throwing an error
- Check order of GeneratePath for faster checks
- Now uses HashSet for checking points (Much faster than a List)

## Fixes
- Now uses a loop instead of recursion to avoid StackOverflowException on big levels
- Prevent trapping checked heigt instead of width

### 1.0 - 2019-06-10
## Added
- Initial release