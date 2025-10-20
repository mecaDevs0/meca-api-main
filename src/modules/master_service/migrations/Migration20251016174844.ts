import { Migration } from '@mikro-orm/migrations';

export class Migration20251016174844 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "master_service" ("id" text not null, "name" text not null, "description" text null, "category" text check ("category" in ('manutencao_preventiva', 'manutencao_corretiva', 'troca_oleo', 'freios', 'suspensao', 'motor', 'eletrica', 'ar_condicionado', 'alinhamento_balanceamento', 'outros')) not null, "estimated_duration_minutes" integer null, "icon_url" text null, "image_url" text null, "suggested_price_min" integer null, "suggested_price_max" integer null, "is_active" boolean not null default true, "display_order" integer not null default 0, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "master_service_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_master_service_deleted_at" ON "master_service" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "master_service" cascade;`);
  }

}
