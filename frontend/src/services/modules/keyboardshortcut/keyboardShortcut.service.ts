import { AxiosRequestConfig } from 'axios';
import { IKeyboardShorcutRequest } from './interfaces/keyboardShortcut.interface';
import httpService from '@/services/httpService';
import { KEYBOARD_SHORTCUT_URL } from '@/consts/apiUrl';
import { KeyboardShortcutFormValues } from '@/pages/admin/keyboardshortcut/dialogs/DialogAddEditShortCut';

class KeyboardShortcutService {
  getAllKeyboardShortcuts(filter: IKeyboardShorcutRequest, config: AxiosRequestConfig) {
    const params = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 50).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
    });

    if (filter?.IsActive !== undefined) {
      params.append('IsActive', filter.IsActive.toString());
    }

    if (filter?.RiskLevel !== undefined) {
      params.append('RiskLevel', filter.RiskLevel.toString());
    }

    return httpService.get(`${KEYBOARD_SHORTCUT_URL}/GetAll?${params.toString()}`, config);
  }

  getDetailKeyboardShortcut(keyId: string) {
    return httpService.get(`${KEYBOARD_SHORTCUT_URL}/GetOne/${keyId}`);
  }

  createUpdateKeyboardShortcut(body: KeyboardShortcutFormValues) {
    return httpService.post(`${KEYBOARD_SHORTCUT_URL}/CreateUpdate`, body);
  }

  deleteKeyboardShortcut(keyId: string[]) {
    return httpService.delete(`${KEYBOARD_SHORTCUT_URL}/Delete`, {
      data: keyId,
    });
  }

  changeStatusKeyboardShortcut(keyId: string[]) {
    return httpService.post(`${KEYBOARD_SHORTCUT_URL}/ChangeActivate`, keyId);
  }
}

export default new KeyboardShortcutService();
