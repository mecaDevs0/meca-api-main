import { Migration } from '@mikro-orm/migrations';

export class Migration20251016223259 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "vehicle_history" ("id" text not null, "vehicle_id" text not null, "booking_id" text not null, "service_name" text not null, "workshop_name" text not null, "service_date" timestamptz not null, "odometer_reading" integer null, "next_service_km" integer null, "next_service_date" timestamptz null, "notes" text null, "parts_replaced" jsonb null, "labor_hours" integer null, "parts_cost" integer not null, "labor_cost" integer not null, "total_cost" integer not null, "warranty_months" integer not null default 3, "warranty_expires_at" timestamptz null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "vehicle_history_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_vehicle_history_deleted_at" ON "vehicle_history" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "vehicle_history" cascade;`);
  }

}
