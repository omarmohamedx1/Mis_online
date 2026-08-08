import { LogIn } from 'lucide-react';
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Button } from '../../../components/common/Button';
import { FormError } from '../../../components/common/FormError';
import { Checkbox } from '../../../components/forms/Checkbox';
import { PasswordInput } from '../../../components/forms/PasswordInput';
import { TextInput } from '../../../components/forms/TextInput';
import { useAuth } from '../../../context/AuthContext';
import { getApiErrorMessage } from '../../../services/apiClient';
import type { LoginFormValues } from '../types/auth';
import { validateLogin, type LoginValidationErrors } from '../validation/loginValidation';

interface RouteState {
  from?: {
    pathname?: string;
  };
}

export function LoginForm() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const routeState = location.state as RouteState | null;
  const redirectTo = routeState?.from?.pathname ?? '/dashboard';

  const [values, setValues] = useState<LoginFormValues>({
    username: '',
    password: '',
    rememberMe: false,
  });
  const [errors, setErrors] = useState<LoginValidationErrors>({});
  const [formError, setFormError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError('');

    const validationErrors = validateLogin(values);
    setErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setIsSubmitting(true);

    try {
      await login(
        {
          username: values.username.trim(),
          password: values.password,
        },
        values.rememberMe,
      );
      navigate(redirectTo, { replace: true });
    } catch (error) {
      setFormError(getApiErrorMessage(error, 'Invalid username or password.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="space-y-5" noValidate onSubmit={handleSubmit}>
      <FormError message={formError} />

      <TextInput
        autoComplete="username"
        error={errors.username}
        label="Email / Username"
        name="username"
        onChange={(event) => setValues((current) => ({ ...current, username: event.target.value }))}
        placeholder="admin or name@company.com"
        required
        value={values.username}
      />

      <PasswordInput
        autoComplete="current-password"
        error={errors.password}
        label="Password"
        name="password"
        onChange={(event) => setValues((current) => ({ ...current, password: event.target.value }))}
        placeholder="Enter your password"
        required
        value={values.password}
      />

      <div className="flex items-center justify-between gap-4">
        <Checkbox
          checked={values.rememberMe}
          label="Remember me"
          name="rememberMe"
          onChange={(event) => setValues((current) => ({ ...current, rememberMe: event.target.checked }))}
        />
        <a className="text-sm font-semibold text-mis-primary transition hover:text-mis-deep" href="/login">
          Forgot Password?
        </a>
      </div>

      <Button isLoading={isSubmitting} leftIcon={<LogIn className="h-4 w-4" aria-hidden="true" />} type="submit">
        {isSubmitting ? 'Signing In' : 'Sign In'}
      </Button>
    </form>
  );
}
