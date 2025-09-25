
import { FileText } from "lucide-react";
import { useTranslation } from "react-i18next";

export function Header() {
  const { t } = useTranslation('shared');
  return (
    <div className="border-b border-gray-200 bg-white px-6 py-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600">
              <FileText className="h-4 w-4 text-white" />
            </div>
            <div>
              <h1 className="text-xl font-semibold text-gray-900">
                {t('AddQuestion.Title')}
              </h1>
              <p className="text-sm text-gray-500">
                {t('AddQuestion.SubTitle')}
              </p>
            </div>
          </div>
        </div>

      </div>


    </div>
  );
}
