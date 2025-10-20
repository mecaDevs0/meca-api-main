import { Migration } from '@mikro-orm/migrations';

export class Migration20251016154013 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`alter table if exists "oficina" drop constraint if exists "oficina_cnpj_unique";`);
    this.addSql(`create table if not exists "oficina" ("id" text not null, "name" text not null, "cnpj" text not null, "email" text not null, "phone" text not null, "address" jsonb null, "logo_url" text null, "photo_urls" jsonb null, "description" text null, "dados_bancarios" jsonb null, "horario_funcionamento" jsonb null, "status" text check ("status" in ('pendente', 'aprovado', 'rejeitado', 'suspenso')) not null default 'pendente', "status_reason" text null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "oficina_pkey" primary key ("id"));`);
    this.addSql(`CREATE UNIQUE INDEX IF NOT EXISTS "IDX_oficina_cnpj_unique" ON "oficina" (cnpj) WHERE deleted_at IS NULL;`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_oficina_deleted_at" ON "oficina" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "oficina" cascade;`);
  }

}
