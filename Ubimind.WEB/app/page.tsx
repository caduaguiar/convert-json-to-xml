import FileUpload from '@/components/FileUpload';

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
      <div className="container mx-auto px-4 py-16">
        <div className="max-w-2xl mx-auto">
          <div className="text-center mb-12">
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
              JSON to XML Converter
            </h1>
            <p className="text-lg text-gray-600 dark:text-gray-400">
              Upload your JSON file and convert it to XML format
            </p>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8">
            <FileUpload />
          </div>

          <div className="mt-8 text-center text-sm text-gray-500 dark:text-gray-400">
            <p>Supported format: .json files</p>
            <p className="mt-2">The converted XML file will be downloaded automatically</p>
          </div>
        </div>
      </div>
    </div>
  );
}
