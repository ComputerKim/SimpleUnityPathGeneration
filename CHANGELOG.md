### 1.1 - 2019-06-15
## Added
- Github
- Will now only check a point once, this speeds things up

## Changes
- When reaching max loops the path will go down instead of throwing an error
- Now uses HashSet for checking points (Much faster than a List)
- A lot of other small things

## Fixes
- Now uses a loop instead of recursion to avoid StackOverflowException on big levels
- Prevent trapping checked heigt instead of width

### 1.0 - 2019-06-10
## Added
- Initial release
