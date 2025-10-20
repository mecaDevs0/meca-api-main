import { Migration } from '@mikro-orm/migrations';

export class Migration20251016154014 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "booking" ("id" text not null, "appointment_date" timestamptz not null, "customer_id" text not null, "vehicle_id" text not null, "oficina_id" text not null, "product_id" text not null, "order_id" text null, "status" text check ("status" in ('pendente_oficina', 'confirmado', 'recusado', 'finalizado_mecanico', 'finalizado_cliente', 'cancelado', 'nao_compareceu')) not null default 'pendente_oficina', "status_history" jsonb null, "vehicle_snapshot" jsonb null, "customer_notes" text null, "oficina_notes" text null, "estimated_price" integer null, "final_price" integer null, "confirmed_at" timestamptz null, "completed_at" timestamptz null, "cancelled_at" timestamptz null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "booking_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_booking_deleted_at" ON "booking" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "booking" cascade;`);
  }

}
