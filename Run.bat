:run.bat

cd  Debug/

START ServerPrototype.exe

cd ../

cd  GUI/bin/x86/Debug

START WpfApp1.exe 8081


START WpfApp1.exe 8082


@echo:

cd ../../../../