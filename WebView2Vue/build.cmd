set "OUTPUT_DIR=.\dist"

if exist "%OUTPUT_DIR%" (
    rd /s /q "%OUTPUT_DIR%"
)

npm run build