import type { LoginFormValues } from '../types/auth';

export interface LoginValidationErrors {
  username?: string;
  password?: string;
}

export function validateLogin(values: LoginFormValues, messages: { usernameRequired: string; passwordRequired: string }): LoginValidationErrors {
  const errors: LoginValidationErrors = {};

  if (!values.username.trim()) {
    errors.username = messages.usernameRequired;
  }

  if (!values.password) {
    errors.password = messages.passwordRequired;
  }

  return errors;
}
