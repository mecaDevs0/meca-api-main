import { Migration } from '@mikro-orm/migrations';

export class Migration20251016174844 extends Migration {

  override async up(): Promise<void> {
    this.addSql(`create table if not exists "review" ("id" text not null, "customer_id" text not null, "oficina_id" text not null, "booking_id" text not null, "product_id" text null, "rating" integer not null, "title" text null, "comment" text null, "oficina_response" text null, "oficina_response_at" timestamptz null, "is_approved" boolean not null default true, "is_flagged" boolean not null default false, "moderator_notes" text null, "created_at" timestamptz not null default now(), "updated_at" timestamptz not null default now(), "deleted_at" timestamptz null, constraint "review_pkey" primary key ("id"));`);
    this.addSql(`CREATE INDEX IF NOT EXISTS "IDX_review_deleted_at" ON "review" (deleted_at) WHERE deleted_at IS NULL;`);
  }

  override async down(): Promise<void> {
    this.addSql(`drop table if exists "review" cascade;`);
  }

}
