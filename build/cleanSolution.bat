for /d /r "%~dp0..\" %%a in (obj\) do if exist "%%a" rmdir /s /q "%%a"
for /d /r "%~dp0..\" %%a in (bin\) do if exist "%%a" rmdir /s /q "%%a"
:: uncomment below to keep cmd window open after executing
:: cmd /k