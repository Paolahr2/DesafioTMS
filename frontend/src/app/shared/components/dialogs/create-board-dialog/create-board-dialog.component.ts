import { Component, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface CreateBoardData {
  title: string;
  description: string;
  color: string;
  isPublic: boolean;
}

@Component({
  selector: 'app-create-board-dialog',
  standalone: true,
  encapsulation: ViewEncapsulation.Emulated, // Cambiado de None a Emulated para evitar conflictos
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatDividerModule,
    MatTooltipModule
  ],
  templateUrl: './create-board-dialog.component.html',
  styleUrls: ['./create-board-dialog.component.scss']
})
export class CreateBoardDialogComponent {
  boardForm!: FormGroup; // Usar definite assignment assertion
  isLoading = false;

  availableColors = [
    { name: 'Naranja Suave', value: '#d97706' },
    { name: 'Azul Suave', value: '#2563eb' },
    { name: 'Verde Suave', value: '#059669' },
    { name: 'Rojo Suave', value: '#dc2626' },
    { name: 'Morado Suave', value: '#7c3aed' },
    { name: 'Rosa Suave', value: '#db2777' },
    { name: 'Turquesa Suave', value: '#0891b2' },
    { name: 'Lima Suave', value: '#65a30d' },
    { name: 'Índigo Suave', value: '#4f46e5' },
    { name: 'Ámbar Suave', value: '#d97706' },
    { name: 'Esmeralda Suave', value: '#047857' },
    { name: 'Cian Suave', value: '#0e7490' },
    { name: 'Violeta Suave', value: '#6d28d9' },
    { name: 'Fucsia Suave', value: '#a21caf' },
    { name: 'Gris', value: '#6b7280' },
    { name: 'Gris Pizarra', value: '#475569' },
    { name: 'Gris Oscuro', value: '#374151' },
    { name: 'Negro Suave', value: '#1f2937' },
    { name: 'Morado Oscuro', value: '#5b21b6' },
    { name: 'Azul Marino', value: '#1e40af' },
    { name: 'Verde Oscuro', value: '#166534' },
    { name: 'Rojo Oscuro', value: '#991b1b' }
  ];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateBoardDialogComponent>,
    private cdr: ChangeDetectorRef
  ) {
    this.initializeForm();
  }

  private initializeForm() {
    this.boardForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      color: ['#f97316', Validators.required],
      isPublic: [false]
    });

    // Forzar detección de cambios después de inicializar
    setTimeout(() => {
      this.cdr.detectChanges();
    }, 0);
  }

  ngOnInit() {
    // Reinicializar el formulario cuando se abre el diálogo
    this.initializeForm();
  }

  selectColor(colorValue: string) {
    this.boardForm.get('color')?.setValue(colorValue);
    this.cdr.detectChanges();
  }

  selectPrivacy(isPublic: boolean) {
    this.boardForm.get('isPublic')?.setValue(isPublic);
    this.cdr.detectChanges();
  }

  trackByColorValue(index: number, color: any): string {
    return color.value;
  }

  getSelectedColorName(): string | null {
    const selectedValue = this.boardForm.get('color')?.value;
    const selectedColor = this.availableColors.find(color => color.value === selectedValue);
    return selectedColor ? selectedColor.name : null;
  }

  onSubmit() {
    if (this.boardForm.valid) {
      this.isLoading = true;
      const formData = this.boardForm.value;
      console.log('Datos del formulario:', formData);
      this.dialogRef.close(formData);
    } else {
      // Marcar todos los campos como touched para mostrar errores
      Object.keys(this.boardForm.controls).forEach(key => {
        this.boardForm.get(key)?.markAsTouched();
      });
      this.cdr.detectChanges();
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}


