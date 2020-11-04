cd %~dp0
del test.blob
del test.blob.gz2
del test.blob.copy
fsutil file createnew test.blob 12000000000
%~dp0\..\GZipTest.Console\bin\Debug\GZipTest.exe compress %~dp0\test.blob %~dp0\test.blob.gz2
%~dp0\..\GZipTest.Console\bin\Debug\GZipTest.exe decompress %~dp0\test.blob.gz2 %~dp0\test.blob.copy
fc /b test.blob test.blob.copy
pause