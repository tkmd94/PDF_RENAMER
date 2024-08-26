# PDF_RENAMER

"PDF_RENAMER" is a tool for renaming specified PDF files. The program extracts text from PDF files based on specified patterns and generates new filename based on that text.

# Features

* **Dynamic Renaming Based on Patterns**: Extracts text from PDFs based on XML pattern files and dynamically generates filenames.
* **Text Extraction Using Regular Expressions**: Extracts text using regular expressions based on start and end strings.
* **Support for Multiple Pattern Files**: Supports multiple pattern files and explores applicable patterns for each PDF.
* **Integration with Windows SendTo Menu**: Allows processing of selected files directly from the SendTo menu by creating a shortcut for the program.
* **Debug Mode**: Skip renaming and save extracted text to a file using the `/debug` option.

# Demo

None

# Requirement

None

# Installation

1. Copy the PDF_RENAMER folder, which includes PDF_RENAMER.exe, to any location on your PC.
2. Copy the PatternFolder to the PDF_RENAMER folder.
3. Open Windows Explorer, type "shell:sendto" in the address bar, and the SendTo folder will open.
4. Create a shortcut for PDF_RENAMER.exe within the PDF_RENAMER folder and save it in the SendTo folder.

# Usage

**Note: Use this source code at your own risk.**

1. Select the file(s) you want to rename, right-click, and choose PDF_RENAMER from the SendTo menu.
2. Confirm that the selected file(s) have been renamed correctly.

* The renaming process will only be executed if all patterns are matched when multiple patterns are defined.
* You can specify any pattern by adding or modifying pattern files within the PatternFolder.
* You can save extracted text to a file by specifying the debug option:
  ```
  PDF_RENAMER.exe /debug AAA.pdf BBB.pdf
  ```

### Pattern File Definition
Explanation of Tags:
* **Pattern**: Defines the regular expression search pattern. The search pattern is ` <Start>.*<End> `, and the extracted string corresponds to the part matching `.*`.
   * **Keyword**: The keyword (name) that characterizes the pattern.
   * **Start**: The start string for the search.
   * **End**: The end string for the search.
* **Order**: Defines the concatenation order of the extracted strings. Strings are concatenated with an underscore (_).
   * **Keyword**: Written using the Keyword defined in the Pattern tag. It is not necessary to use all Keywords.

# Author

* Takashi Kodama

# License

"PDF_RENAMER" is under the [MIT License](https://en.wikipedia.org/wiki/MIT_License) .
