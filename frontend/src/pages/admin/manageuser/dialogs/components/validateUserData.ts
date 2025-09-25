import { dobRegex, emailRegex, phoneRegex } from '@/consts/regex';
import {
  REQUIRED_FIELDS,
  UserData,
  VALID_ROLES,
  ValidationError,
} from '../DialogPreCheckImportUser';

const validateEmail = (email: string): boolean => emailRegex.test(email);

const validatePhone = (phone: string): boolean => phoneRegex.test(phone);

const validateDob = (dob: string): boolean => {
  if (!dob || dob.trim() === '') return true;

  // Check DD/MM/YYYY format
  if (dobRegex.test(dob)) {
    const [day, month, year] = dob.split('/').map(Number);
    const date = new Date(year, month - 1, day);
    const today = new Date();

    return (
      date.getFullYear() === year &&
      date.getMonth() === month - 1 &&
      date.getDate() === day &&
      date < today
    );
  }

  // Check Excel serial date
  if (!isNaN(Number(dob))) {
    const serial = Number(dob);
    if (serial > 0) {
      const excelEpoch = new Date(1900, 0, 0);
      const date = new Date(excelEpoch.getTime() + serial * 24 * 60 * 60 * 1000);
      const today = new Date();
      return !isNaN(date.getTime()) && date < today;
    }
  }

  return false;
};

const validateUserData = (data: UserData[]): ValidationError[] => {
  const errors: ValidationError[] = [];
  const seenEmails = new Set<string>();

  data.forEach((user, index) => {
    REQUIRED_FIELDS.forEach((field) => {
      if (!user[field] || user[field].trim() === '') {
        errors.push({ row: index + 2, field, message: `Missing required field: ${field}` });
      }
    });

    if (user.Email) {
      if (seenEmails.has(user.Email.toLowerCase())) {
        errors.push({ row: index + 2, field: 'Email', message: 'Duplicate email found.' });
      } else {
        seenEmails.add(user.Email.toLowerCase());
      }

      if (!validateEmail(user.Email)) {
        errors.push({ row: index + 2, field: 'Email', message: 'Invalid email format.' });
      }
    }

    if (user.Phone && !validatePhone(user.Phone)) {
      errors.push({ row: index + 2, field: 'Phone', message: 'Invalid phone number.' });
    }

    if (user.RoleId && !VALID_ROLES.includes(user.RoleId)) {
      errors.push({ row: index + 2, field: 'RoleId', message: 'Invalid role assignment.' });
    }

    if (user.RoleId === '1' && (!user.MajorId || user.MajorId.trim() === '')) {
      errors.push({
        row: index + 2,
        field: 'MajorId',
        message: 'Please select a major for this student.',
      });
    }

    if (user.RoleId === '2') {
      if (!user.DepartmentId || user.DepartmentId.trim() === '') {
        errors.push({
          row: index + 2,
          field: 'DepartmentId',
          message: 'Please select a department.',
        });
      }
      if (!user.SpecializationId || user.SpecializationId.trim() === '') {
        errors.push({
          row: index + 2,
          field: 'SpecializationId',
          message: 'Please provide specialization for teacher.',
        });
      }
    }

    if (
      ['2', '3', '4'].includes(user.RoleId) &&
      (!user.PositionId || user.PositionId.trim() === '')
    ) {
      errors.push({
        row: index + 2,
        field: 'PositionId',
        message: 'Please provide a position for this role.',
      });
    }

    if (user.Dob && !validateDob(user.Dob)) {
      errors.push({
        row: index + 2,
        field: 'Dob',
        message: 'Invalid date of birth. Use format DD/MM/YYYY or a valid Excel date.',
      });
    }
  });

  return errors;
};

export default validateUserData;
