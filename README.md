README — Delivery Time Estimator

A command-line engine that maximizes delivery efficiency using Kiki’s vehicle fleet.

Assign packages by weight capacity
Prefer heavier packages when counts tie
Vehicles available once returned to source
Computes delivery time per package

Input Format

<base_delivery_cost> <no_of_packages>
<pkg_id> <weight_kg> <distance_km> <offer_code>

Output Format

<pkg_id> <discount> <total_cost> <estimated_delivery_time_hrs>

Example output

PKG1 0 750 3.98
PKG2 0 1475 1.78
PKG3 0 2350 1.42
PKG4 105 1395 0.85
PKG5 0 2125 4.19

Formulas

Time (hrs) = Distance / Speed
New Available Time = Current Time + (2 × delivery_time)

Running the app

dotnet run

Run Tests

dotnet test

