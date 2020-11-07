cd %~dp0
del test.blob
del test.blob.gz2
del test.blob.copy
fsutil file createnew test.blob 1000000000
..\GZipTest.Console\bin\Debug\GZipTest.exe compress test.blob test.blob.gz2
..\GZipTest.Console\bin\Debug\GZipTest.exe decompress test.blob.gz2 test.blob.copy
fc /b test.blob test.blob.copy
pause