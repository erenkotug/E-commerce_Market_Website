sc create Northwind_Services binPath= "C:\Users\Eren\Desktop\NorthWind\NorthwindService\Northwind_Services.exe"
sc start Northwind_Services
sc config Northwind_Services start= auto
sc failure Northwind_Services reset= 120 actions= restart/60000/restart/60000/restart/60000/""/0


