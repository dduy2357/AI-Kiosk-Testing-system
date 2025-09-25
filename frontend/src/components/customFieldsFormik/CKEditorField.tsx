import { useField } from 'formik';
import { CKEditor } from '@ckeditor/ckeditor5-react';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

const CKEditorField: React.FC<{ name: string; label: string }> = ({ name, label }) => {
  const [field, , helpers] = useField(name);

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      <CKEditor
        editor={ClassicEditor as any}
        data={field.value ?? ''}
        onChange={(_event, editor) => {
          const data = editor.getData();
          helpers.setValue(data);
        }}
        config={{
          toolbar: [
            'heading',
            '|',
            'bold',
            'italic',
            'link',
            'bulletedList',
            'numberedList',
            '|',
            'undo',
            'redo',
          ],
          placeholder: 'Nhập hướng dẫn làm bài...',
        }}
        onReady={(editor) => {
          if (editor.editing.view.document.getRoot()) {
            editor.editing.view.change((writer) => {
              const root = editor.editing.view.document.getRoot();
              if (root) {
                writer.setStyle('min-height', '150px', root);
              }
            });
          }
        }}
      />
    </div>
  );
};

export default CKEditorField;
