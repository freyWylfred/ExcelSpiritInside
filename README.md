# Excel Spirit Inside

A Windows desktop application for comparing two Excel workbooks and reasoning over spreadsheet data with a fully local Large Language Model (LLM). No cloud services are required — inference runs entirely on your machine.

## Features

- **Excel Diff**: Compare a named worksheet across two Excel files and detect every cell-level difference.
- **Diff Export**: Save the detected differences to a brand-new Excel workbook (`Sheet`, `Cell`, `File 1`, `File 2` columns).
- **Hidden Row/Column Awareness**: Rows and columns that are hidden in either workbook are automatically excluded from the comparison.
- **Local LLM Inference**: On first launch the app downloads the **Qwen3 4B** model (GGUF) and runs it locally via [LLamaSharp](https://github.com/SciSharp/LLamaSharp). If the model is already present, the download is skipped.
- **Column Inference**: Point the model at a specific column (e.g. `A`) and get an AI-generated summary of its contents.
- **Free-form Instructions**: Send any natural-language instruction to the local model and view its response.
- **Modern UI**: Clean, DPI-aware interface built with a responsive layout.

## Requirements

- Windows 10/11 (x64)
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- Internet connection on first launch (to download the model)
- ~3 GB of free disk space for the model file

## Getting Started

1. Download the latest release from the [Releases](../../releases) page and extract it.
2. Run `ExcelSpiritInside.exe`.
3. On first launch, wait for the Qwen3 4B model to download. Progress is shown in the status bar.

The model is stored under:

```
%LOCALAPPDATA%\ExcelSpiritInside\models\Qwen3-4B-Q4_K_M.gguf
```

## Usage

### Compare two Excel files

1. Select **Excel File 1** and **Excel File 2** using the **Browse...** buttons.
2. Enter the **Sheet Name** to compare.
3. Click **Compare**. If differences are found, choose where to save the resulting diff workbook.

Hidden rows and columns are ignored during comparison.

### Infer a column

1. Select an Excel file and enter the **Sheet Name**.
2. Enter the column letter (e.g. `A`) in **Infer Column**.
3. Click **Infer** to get an AI summary of that column.

### Send an instruction to the local model

1. Type any instruction into the **Instruction** box.
2. Click **Ask** to view the model's response.

## Building from Source

```powershell
dotnet build ExcelSpiritInside\ExcelSpiritInside.csproj -c Release
```

## Tech Stack

- .NET 10 / Windows Forms
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) for Excel reading/writing
- [LLamaSharp](https://github.com/SciSharp/LLamaSharp) for local LLM inference
- Qwen3 4B (GGUF, Q4_K_M quantization)

## License

MIT
