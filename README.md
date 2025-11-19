# Simplex Runner

A program that given a maximizing LP problem(example shown below) in tableau/structured or just raw model form, produce the full working and the solution

Example of LP:
```
MAX x+3y
ST
   x + y <= 100
   2x + y >= 120
END
```
(^Side note this is also the raw model form)
Which the optimal solution would be `x = 20, y = 80` with `P = 260`, which is the maximum value for the given constraints (x, y have been implies to be >= 0)
