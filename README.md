# ClearShift Currency Comparison Tool

## Abstract

The ClearShift Currency Comparison Tool is a sophisticated web application developed in ASP.NET Core that provides comprehensive analysis and comparison of international currency exchange transactions. The system integrates real-time Bank of Israel exchange rate data with advanced PDF document processing capabilities to deliver transparent cost analysis for international money transfers.

## Project Overview

### Purpose and Scope

This application serves as an analytical platform designed to empower users in making informed decisions regarding international currency exchanges. By comparing traditional banking rates with ClearShift's competitive exchange offerings, the tool provides clear visibility into potential savings and transaction costs.

### Key Features

- **Real-Time Exchange Rate Integration**: Direct API integration with Bank of Israel exchange rate services for accurate, up-to-date currency conversion data
- **Advanced PDF Document Processing**: Intelligent extraction and field mapping from bank statements and financial documents
- **Interactive Field Selection Interface**: When automated PDF field recognition encounters limitations, the system provides a sophisticated manual selection mechanism
- **Comprehensive Cost Analysis**: Detailed breakdown of fees, rates, and potential savings across different service providers
- **Multi-Format Document Support**: Processing capabilities for PDF, CSV, and text-based financial documents
- **Professional Report Generation**: PDF export functionality for detailed comparison reports

## Technical Architecture

### Core Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Runtime**: .NET 9.0
- **Architecture Pattern**: Model-View-Controller (MVC)
- **Frontend Technologies**: HTML5, CSS3, JavaScript ES6+
- **PDF Processing**: PDF.js library with custom viewer implementation
- **Document Generation**: PDFsharp-MigraDoc for report creation

### Dependencies and Libraries

```xml
<PackageReference Include="PDFsharp-MigraDoc-gdi" Version="1.50.5147" />
<PackageReference Include="System.Drawing.Common" Version="9.0.0" />
<PackageReference Include="System.Text.Encoding.CodePages" Version="9.0.7" />
```

### System Components

#### 1. Exchange Rate Service Layer
The `BOIExchangeRateService` implements the `IExchangeRateService` interface to provide:
- Direct rate retrieval from Bank of Israel APIs
- Cross-currency rate calculations
- Historical rate data access
- Rate validation and error handling

#### 2. PDF Processing Engine
The application incorporates a sophisticated PDF processing system consisting of:
- **CleanPDFViewer**: Custom PDF rendering engine built on PDF.js
- **Field Mapping Interface**: Interactive text selection and field association
- **Text Recognition**: Automated parsing of financial document structures

#### 3. Document Field Mapping System
When automated field recognition encounters challenges in identifying specific data points within PDF documents, the system gracefully transitions to an interactive manual selection interface. This advanced fallback mechanism allows users to:
- Select text directly from rendered PDF documents
- Map selected content to specific transaction fields (date, amount, currencies, rates, fees)
- Validate and confirm field associations before processing
- Clear and re-map fields as needed

## Installation and Setup

### Prerequisites

- .NET 9.0 SDK or later
- Windows operating system with GDI+ support
- Modern web browser with JavaScript enabled

### Configuration

1. Clone the repository to your local development environment
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Configure application settings in `appsettings.json` for environment-specific parameters
4. Build the application:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

## Usage Guide

### Manual Currency Comparison

1. Navigate to the main interface
2. Enter transaction details:
   - Transaction amount
   - Source and target currencies
   - Transaction date
   - Bank exchange rate
   - Associated fees (optional)
3. Review the comprehensive comparison results

### PDF Document Processing

1. Upload a bank statement or financial document (PDF, CSV, or TXT format)
2. The system automatically attempts to recognize and extract relevant fields
3. If automatic recognition is incomplete, the interactive field mapping interface activates:
   - Review the rendered PDF document in the integrated viewer
   - Select text fragments by clicking or dragging within the document
   - Click on the appropriate field buttons to associate selected text with transaction parameters
   - Validate mappings and apply to the comparison form
4. Process the complete analysis with extracted data

### Advanced Features

- **Currency Pair Selection**: Support for major international currencies with automatic cross-rate calculations
- **Fee Structure Analysis**: Detailed breakdown of percentage-based and fixed fees
- **Savings Calculation**: Precise computation of potential savings between service providers
- **Export Functionality**: Generate professional PDF reports of comparison results

## API Integration

The application interfaces with the Bank of Israel exchange rate API to ensure accuracy and compliance with official financial data sources. The integration handles:
- Rate caching for performance optimization
- Error handling for service unavailability
- Historical rate retrieval for backdated transactions
- Cross-currency calculations for non-shekel pairs

## Security Considerations

- Input sanitization for uploaded documents
- Secure file handling with size and type restrictions
- Client-side validation with server-side verification
- Memory management for PDF processing operations

## Performance Optimization

- Asynchronous PDF processing operations
- Efficient text layer rendering
- Optimized field mapping algorithms
- Responsive design for cross-device compatibility

## Contributing

This project follows standard software engineering practices including:
- Clean architecture principles
- Comprehensive error handling
- Extensible service interfaces
- Maintainable code structure

## License

This project is developed for ClearShift financial services and is subject to proprietary licensing terms.

## Technical Support

For technical inquiries, implementation guidance, or system integration support, please consult the development team documentation or submit detailed issue reports through the appropriate channels.
