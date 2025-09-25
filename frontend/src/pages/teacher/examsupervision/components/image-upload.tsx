import type React from 'react';

import { Label } from '@/components/ui/label';
import { Clipboard, Upload, X } from 'lucide-react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface ImageUploadProps {
  uploadedFile: File | null;
  onFileUpload: (file: File | null) => void;
}

export default function ImageUpload({ uploadedFile, onFileUpload }: Readonly<ImageUploadProps>) {
  const { t } = useTranslation('shared');
  const [dragActive, setDragActive] = useState(false);

  const handleFileUpload = (files: FileList | null) => {
    if (!files || files.length === 0) return;

    const file = files[0]; // Only take the first file
    const isImage = file.type.startsWith('image/');
    const isValidSize = file.size <= 10 * 1024 * 1024; // 10MB limit

    if (isImage && isValidSize) {
      onFileUpload(file);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    handleFileUpload(e.dataTransfer.files);
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    const items = e.clipboardData.items;
    for (const item of items) {
      if (item.type.includes('image')) {
        const file = item.getAsFile();
        if (file) {
          onFileUpload(file);
          break;
        }
      }
    }
  };

  const removeFile = () => {
    onFileUpload(null);
  };

  const getFilePreview = (file: File): string => {
    return URL.createObjectURL(file);
  };

  return (
    <div className="space-y-2">
      <Label className="text-sm font-medium">{t('ExamSupervision.ProofOfViolation')}</Label>

      {/* Upload Area */}
      <label
        htmlFor="file-upload"
        className={`rounded-lg border-2 border-dashed p-6 text-center transition-colors ${
          dragActive ? 'border-blue-400 bg-blue-50' : 'border-gray-300 hover:border-gray-400'
        } cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-400`}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
        onPaste={handlePaste}
      >
        <div className="space-y-4">
          <div className="flex flex-col items-center gap-2">
            <Upload className="h-8 w-8 text-gray-400" />
            <div className="text-sm text-gray-600">
              <p>{t('ExamSupervision.DragAndDropOrClick')}</p>
              <span className="font-medium text-blue-600 hover:text-blue-700">
                {t('ExamSupervision.SelectFile')}
              </span>
            </div>
          </div>

          <div className="flex items-center gap-2 text-xs text-gray-500">
            <Clipboard className="h-4 w-4" />
            <span>{t('ExamSupervision.PasteImage')}</span>
          </div>

          <p className="text-xs text-gray-400">{t('ExamSupervision.FileSizeLimit')}</p>
        </div>

        <input
          id="file-upload"
          type="file"
          accept="image/*"
          className="hidden"
          onChange={(e) => handleFileUpload(e.target.files)}
        />
      </label>

      {/* File Preview */}
      {uploadedFile && (
        <div className="space-y-3">
          <p className="text-sm font-medium text-gray-700">{t('ExamSupervision.UploadedImage')}</p>
          <div className="group relative w-48">
            <div className="aspect-square overflow-hidden rounded-lg border bg-gray-50">
              <img
                src={getFilePreview(uploadedFile)}
                alt="Preview"
                className="h-full w-full object-cover"
              />
            </div>
            <button
              type="button"
              onClick={removeFile}
              className="absolute -right-2 -top-2 rounded-full bg-red-500 p-1 text-white opacity-0 transition-opacity group-hover:opacity-100"
            >
              <X className="h-3 w-3" />
            </button>
            <p className="mt-1 truncate text-xs text-gray-500">{uploadedFile.name}</p>
          </div>
        </div>
      )}
    </div>
  );
}
