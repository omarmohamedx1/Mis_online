import type { LoginFormValues } from '../types/auth';

export interface LoginValidationErrors {
  username?: string;
  password?: string;
}

export function validateLogin(values: LoginFormValues): LoginValidationErrors {
  const errors: LoginValidationErrors = {};

  if (!values.username.trim()) {
    errors.username = 'Email or username is required.';
  }

  if (!values.password) {
    errors.password = 'Password is required.';
  }

  return errors;
}
