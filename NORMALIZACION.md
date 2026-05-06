# Normalización – Módulo de Gestión Financiera
## Hospital de Tercer Nivel – Sistemas de Información II

---

## Forma No Normal (F0) – Tabla sin normalizar

Se parte de una tabla gigante con todos los datos del módulo financiero tal como podrían llegar de un formulario o reporte:

| Id | CodFactura | FechaEmision | FechaVenc | CodPaciente | MontoTotal | MontoCobertura | MontoPaciente | EstadoFactura | CodConvenio | NombreAseguradora | TipoCobertura | PorcCobertura | FechaInicioConv | FechaFinConv | Servicios(CodigoSrv, Descripcion, Precio, Subtotal, ...) | Pagos(CodPago, Fecha, Monto, Metodo, Referencia) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|

**Problemas detectados:**
- Grupos repetitivos: múltiples servicios y múltiples pagos dentro de una misma fila.
- Datos de convenio repetidos en cada factura.
- No hay claves primarias claras.

---

## Primera Forma Normal (1FN) – Eliminar grupos repetitivos

**Regla:** Cada columna debe contener valores atómicos (un solo valor). Se eliminan los grupos repetitivos creando filas separadas.

Se separan las entidades principales:

### Tabla FACTURA
| Id | Codigo | CodPaciente | FechaEmision | FechaVencimiento | MontoTotal | MontoCobertura | MontoPaciente | EstadoFactura | Estado | CodConvenio | NombreAseguradora | TipoCobertura | PorcCobertura | FechaInicioConv | FechaFinConv |

### Tabla DETALLE_FACTURA
| Id | Codigo | Id_Factura | CodigoServicio | Descripcion | PrecioUnitario | Subtotal | Estado |

### Tabla PAGO
| Id | Codigo | Id_Factura | FechaPago | Monto | MetodoPago | ReferenciaBancaria | Pagado | Estado |

**Justificación:** Se eliminaron los grupos repetitivos de servicios y pagos. Ahora cada celda contiene un único valor atómico. Sin embargo, en FACTURA aún hay dependencias parciales sobre campos del convenio.

---

## Segunda Forma Normal (2FN) – Eliminar dependencias parciales

**Regla:** Todos los atributos no clave deben depender de **toda** la clave primaria (eliminar dependencias parciales). Solo aplica a tablas con clave compuesta.

En FACTURA, los datos del convenio (NombreAseguradora, TipoCobertura, PorcCobertura, FechaInicio, FechaFin) dependen únicamente de CodConvenio, no de CodFactura.

Se separa CONVENIO en su propia tabla:

### Tabla CONVENIO
| Id | Codigo | NombreAseguradora | TipoCobertura | PorcentajeCoberturaBase | FechaInicio | FechaFin | Estado |

### Tabla FACTURA (actualizada)
| Id | Codigo | CodPaciente | FechaEmision | FechaVencimiento | MontoTotal | MontoCobertura | MontoPaciente | EstadoFactura | Id_Convenio (FK) | Estado |

**Justificación:** Los atributos del convenio ahora residen en su propia tabla. FACTURA referencia al convenio por FK. No hay más dependencias parciales.

---

## Tercera Forma Normal (3FN) – Eliminar dependencias transitivas

**Regla:** Los atributos no clave no deben depender de otros atributos no clave (eliminar dependencias transitivas).

Revisión:
- En FACTURA: `MontoPaciente = MontoTotal - MontoCobertura`. Este campo es derivado. **Se puede calcular**, pero se mantiene desnormalizado por rendimiento y auditoría (lo justificamos).
- En DETALLE_FACTURA: `Subtotal` es derivado de PrecioUnitario × Cantidad, pero se mantiene por auditoría.
- No se detectan dependencias transitivas reales que requieran separación adicional.

Adicionalmente, los aranceles (precios por servicio) de un convenio son datos propios del convenio, no de la factura.

### Nueva tabla: ARANCEL
| Id | Codigo | Id_Convenio (FK) | CodigoServicio | NombreServicio | PrecioBase | PrecioConvenio | Estado |

**Justificación:** Los precios pactados por convenio dependen del convenio y del servicio, no de la factura. Se extrae a su propia tabla eliminando la dependencia transitiva.

---

## Forma Normal de Boyce-Codd (BCNF)

**Regla:** Para cada dependencia funcional X → Y, X debe ser una superclave.

Revisión de todas las tablas:

- **CONVENIO:** `Codigo → todos los atributos`. Codigo es superclave. ✓
- **ARANCEL:** `(Id_Convenio, CodigoServicio) → PrecioBase, PrecioConvenio`. La clave compuesta es superclave. ✓
- **FACTURA:** `Codigo → todos los atributos`. ✓
- **DETALLE_FACTURA:** `(Id_Factura, CodigoServicio) → Descripcion, PrecioUnitario, Subtotal`. ✓
- **PAGO:** `Codigo → todos los atributos`. ✓

**No se detectan violaciones de BCNF.** Todas las dependencias funcionales tienen superclave como determinante.

---

## Cuarta Forma Normal (4FN) – Eliminar dependencias multivaluadas

**Regla:** No debe haber dependencias multivaluadas no triviales.

Una dependencia multivaluada ocurre cuando un atributo A determina independientemente a dos conjuntos B y C (A →→ B y A →→ C).

Revisión:
- Un convenio puede tener múltiples aranceles (servicios) y múltiples facturas. Estas son independientes entre sí: un convenio no determina combinaciones específicas de aranceles y facturas juntos, sino cada uno por separado.
- Solución: ARANCEL y FACTURA ya son tablas separadas, ambas con FK a CONVENIO. Esto elimina la dependencia multivaluada. ✓
- PAGO depende solo de FACTURA. ✓
- DETALLE_FACTURA depende solo de FACTURA. ✓

**No hay violaciones de 4FN en el diseño actual.**

---

## Quinta Forma Normal (5FN) – Eliminar dependencias de join

**Regla:** Toda dependencia de join debe ser implicada por las claves candidatas. No debe ser posible descomponer una tabla en partes más pequeñas sin pérdida de información.

La 5FN se aplica cuando una relación ternaria no puede derivarse de relaciones binarias.

### Caso de aplicación: Arancel como relación 5FN

La tabla **ARANCEL** representa la relación:
> "Un **Convenio** pacta un **Precio** para un **Servicio** específico."

Esta es una relación ternaria genuina: el precio pactado no existe si falta el convenio O si falta el servicio. No se puede dividir en:
- Convenio ↔ Servicio (¿qué servicios tiene el convenio?)
- Convenio ↔ Precio (¿qué precios tiene el convenio?)

porque ambas piezas necesitan estar juntas para tener sentido. Esto **justifica la existencia de ARANCEL como tabla independiente** en 5FN.

### Diseño final en 5FN

| Tabla | Clave Primaria | Claves Foráneas | Justificación 5FN |
|---|---|---|---|
| CONVENIO | Id, Codigo | — | Entidad independiente |
| ARANCEL | Id, Codigo | Id_Convenio → CONVENIO | Relación ternaria: Convenio + Servicio → Precio. No derivable de binarias. |
| FACTURA | Id, Codigo | Id_Convenio → CONVENIO | Entidad independiente con referencia opcional a convenio |
| DETALLE_FACTURA | Id, Codigo | Id_Factura → FACTURA | Relación binaria: Factura + Servicio |
| PAGO | Id, Codigo | Id_Factura → FACTURA | Relación binaria: Factura → Pago |

---

## Diagrama Entidad-Relación Final (descripción)

```
CONVENIO (1) ──────────────────── (N) ARANCEL
    │                                  [Id, Codigo, Id_Convenio, CodigoServicio,
    │                                   NombreServicio, PrecioBase, PrecioConvenio, Estado]
    │
    └────────── (N) FACTURA
                    [Id, Codigo, CodigoPaciente, FechaEmision, FechaVencimiento,
                     MontoTotal, MontoCobertura, MontoPaciente, EstadoFactura,
                     Id_Convenio(FK nullable), Estado]
                         │
                         ├──── (N) DETALLE_FACTURA
                         │         [Id, Codigo, Id_Factura, CodigoServicio,
                         │          Descripcion, PrecioUnitario, Subtotal, Estado]
                         │
                         └──── (N) PAGO
                                   [Id, Codigo, Id_Factura, FechaPago, Monto,
                                    MetodoPago, ReferenciaBancaria, Pagado, Estado]
```

---

## Tablas Finales con todos los campos

### CONVENIO
| Campo | Tipo | Restricción |
|---|---|---|
| Id | INT | PK, auto-increment |
| Codigo | VARCHAR(50) | UNIQUE, NOT NULL |
| NombreAseguradora | VARCHAR(200) | NOT NULL |
| TipoCobertura | VARCHAR(100) | NOT NULL |
| PorcentajeCoberturaBase | DECIMAL(5,2) | NOT NULL |
| FechaInicio | TIMESTAMP | NOT NULL |
| FechaFin | TIMESTAMP | NOT NULL |
| Estado | VARCHAR(20) | DEFAULT 'Activo' |

### ARANCEL (tabla de relación 5FN)
| Campo | Tipo | Restricción |
|---|---|---|
| Id | INT | PK |
| Codigo | VARCHAR(50) | UNIQUE, NOT NULL |
| Id_Convenio | INT | FK → CONVENIO.Id |
| CodigoServicio | VARCHAR(50) | NOT NULL |
| NombreServicio | VARCHAR(200) | NOT NULL |
| PrecioBase | DECIMAL(12,2) | NOT NULL |
| PrecioConvenio | DECIMAL(12,2) | NOT NULL |
| Estado | VARCHAR(20) | DEFAULT 'Activo' |

### FACTURA
| Campo | Tipo | Restricción |
|---|---|---|
| Id | INT | PK |
| Codigo | VARCHAR(50) | UNIQUE, NOT NULL |
| CodigoPaciente | VARCHAR(50) | NOT NULL |
| FechaEmision | TIMESTAMP | NOT NULL |
| FechaVencimiento | TIMESTAMP | NOT NULL |
| MontoTotal | DECIMAL(12,2) | NOT NULL |
| MontoCobertura | DECIMAL(12,2) | DEFAULT 0 |
| MontoPaciente | DECIMAL(12,2) | NOT NULL |
| EstadoFactura | VARCHAR(30) | NOT NULL |
| Id_Convenio | INT | FK → CONVENIO.Id, nullable |
| Estado | VARCHAR(20) | DEFAULT 'Activo' |

### DETALLE_FACTURA
| Campo | Tipo | Restricción |
|---|---|---|
| Id | INT | PK |
| Codigo | VARCHAR(50) | UNIQUE, NOT NULL |
| Id_Factura | INT | FK → FACTURA.Id |
| CodigoServicio | VARCHAR(50) | NOT NULL |
| Descripcion | VARCHAR(300) | NOT NULL |
| PrecioUnitario | DECIMAL(12,2) | NOT NULL |
| Subtotal | DECIMAL(12,2) | NOT NULL |
| Estado | VARCHAR(20) | DEFAULT 'Activo' |

### PAGO
| Campo | Tipo | Restricción |
|---|---|---|
| Id | INT | PK |
| Codigo | VARCHAR(50) | UNIQUE, NOT NULL |
| Id_Factura | INT | FK → FACTURA.Id |
| FechaPago | TIMESTAMP | NOT NULL |
| Monto | DECIMAL(12,2) | NOT NULL |
| MetodoPago | VARCHAR(50) | NOT NULL |
| ReferenciaBancaria | VARCHAR(100) | NOT NULL |
| Pagado | VARCHAR(50) | NOT NULL |
| Estado | VARCHAR(20) | DEFAULT 'Activo' |
