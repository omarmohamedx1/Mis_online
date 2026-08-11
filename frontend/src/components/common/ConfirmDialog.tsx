import { useRef, type ReactNode } from 'react';
import { useLocalization } from '../../context/LocalizationContext';
import { Button, type ButtonVariant } from './Button';
import { Modal, type ModalSize } from './Modal';

export interface ConfirmDialogProps {
  cancelLabel?: ReactNode;
  confirmDisabled?: boolean;
  confirmLabel?: ReactNode;
  confirmVariant?: Extract<ButtonVariant, 'primary' | 'danger'>;
  description?: ReactNode;
  isConfirming?: boolean;
  message: ReactNode;
  onCancel: () => void;
  onConfirm: () => void;
  open: boolean;
  size?: ModalSize;
  title: ReactNode;
}

export function ConfirmDialog({
  cancelLabel,
  confirmDisabled = false,
  confirmLabel,
  confirmVariant = 'danger',
  description,
  isConfirming = false,
  message,
  onCancel,
  onConfirm,
  open,
  size = 'sm',
  title,
}: ConfirmDialogProps) {
  const { t } = useLocalization();
  const cancelButtonRef = useRef<HTMLButtonElement>(null);
  const close = () => {
    if (!isConfirming) onCancel();
  };

  return (
    <Modal
      closeOnBackdrop={!isConfirming}
      closeOnEscape={!isConfirming}
      description={description}
      dialogRole="alertdialog"
      footer={(
        <>
          <Button fullWidth={false} onClick={close} ref={cancelButtonRef} size="md" type="button" variant="outline">
            {cancelLabel ?? t('cancel')}
          </Button>
          <Button disabled={confirmDisabled} fullWidth={false} isLoading={isConfirming} onClick={onConfirm} size="md" type="button" variant={confirmVariant}>
            {confirmLabel ?? t('confirm')}
          </Button>
        </>
      )}
      hideCloseButton={isConfirming}
      initialFocusRef={cancelButtonRef}
      onClose={close}
      open={open}
      size={size}
      title={title}
    >
      <div className="text-sm leading-6 text-slate-600">{message}</div>
    </Modal>
  );
}
