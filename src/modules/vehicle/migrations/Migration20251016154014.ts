import { Migration } from '@mikro-orm/migrations';

export class Migration20251016154014 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "vehicle" ("id" text not null, "marca" text not null, "modelo" text not null, "ano" integer not null, "placa" text not null, "cor" text null, "km_atual" integer null, "combustivel" text null, "observacoes" text null, "customer_id" text not null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "vehicle_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_vehicle_deleted_at" ON "vehicle" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "vehicle" cascade;`);
  }

}
