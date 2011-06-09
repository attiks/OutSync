@Echo off
del *.msi
del *.wixobj

path "C:\Program Files\Windows Installer XML v3\bin"

candle -ext WixNetFxExtension OutSync.wxs
light -ext WixUIExtension -ext WixNetFxExtension -cultures:en-us OutSync.wixobj


del *.wixobj