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

### For Structured Input
You add the amount of variable and constraints at the bottom, and specify the coefficient of each variable on the objective function and each constraints
(We are using the example above)

<img width="3360" height="1624" alt="image" src="https://github.com/user-attachments/assets/f0dd7192-d308-47bb-9184-e9999315bf7b" />

Then it would created the tableau

<img width="3358" height="1622" alt="image" src="https://github.com/user-attachments/assets/9cdd6f8b-f2be-4a8f-8f98-31a08438fb50" />

This allow you to step through the working, and export the tableau at any time

At the end after the working, it will show you the result:

<img width="3360" height="1646" alt="image" src="https://github.com/user-attachments/assets/123873a8-6ea3-49a0-8e1b-523bef7262cb" />

and can export the working with Latex (via Overleaf online or export standalone file)

<img width="2362" height="1870" alt="image" src="https://github.com/user-attachments/assets/d3012261-334e-44a7-8ccd-850f0742097b" />

### For Tableau Input
You basically just type the tableau in instead of the equation for greater freedom on how it run the simplex, but require prior knowledge :)


