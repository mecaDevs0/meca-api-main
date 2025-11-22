import { Migration } from '@mikro-orm/migrations';

export class Migration20251114120000 extends Migration {
  override async up(): Promise<void> {
    this.addSql(`
      ALTER TABLE "customer"
      ADD COLUMN IF NOT EXISTS "auth_provider" varchar(32),
      ADD COLUMN IF NOT EXISTS "google_id" varchar(255),
      ADD COLUMN IF NOT EXISTS "apple_id" varchar(255),
      ADD COLUMN IF NOT EXISTS "avatar_url" varchar(512),
      ADD COLUMN IF NOT EXISTS "provider_metadata" jsonb,
      ADD COLUMN IF NOT EXISTS "last_social_login" timestamptz;
    `);

    this.addSql(`
      ALTER TABLE "workshop"
      ADD COLUMN IF NOT EXISTS "auth_provider" varchar(32),
      ADD COLUMN IF NOT EXISTS "google_id" varchar(255),
      ADD COLUMN IF NOT EXISTS "apple_id" varchar(255),
      ADD COLUMN IF NOT EXISTS "avatar_url" varchar(512),
      ADD COLUMN IF NOT EXISTS "provider_metadata" jsonb,
      ADD COLUMN IF NOT EXISTS "last_social_login" timestamptz;
    `);

    this.addSql(`
      CREATE UNIQUE INDEX IF NOT EXISTS "customer_google_id_unique"
        ON "customer" (LOWER("google_id"))
        WHERE "google_id" IS NOT NULL;
    `);

    this.addSql(`
      CREATE UNIQUE INDEX IF NOT EXISTS "customer_apple_id_unique"
        ON "customer" (LOWER("apple_id"))
        WHERE "apple_id" IS NOT NULL;
    `);

    this.addSql(`
      CREATE UNIQUE INDEX IF NOT EXISTS "workshop_google_id_unique"
        ON "workshop" (LOWER("google_id"))
        WHERE "google_id" IS NOT NULL;
    `);

    this.addSql(`
      CREATE UNIQUE INDEX IF NOT EXISTS "workshop_apple_id_unique"
        ON "workshop" (LOWER("apple_id"))
        WHERE "apple_id" IS NOT NULL;
    `);
  }

  override async down(): Promise<void> {
    this.addSql('DROP INDEX IF EXISTS "customer_google_id_unique";');
    this.addSql('DROP INDEX IF EXISTS "customer_apple_id_unique";');
    this.addSql('DROP INDEX IF EXISTS "workshop_google_id_unique";');
    this.addSql('DROP INDEX IF EXISTS "workshop_apple_id_unique";');

    this.addSql(`
      ALTER TABLE "customer"
      DROP COLUMN IF EXISTS "auth_provider",
      DROP COLUMN IF EXISTS "google_id",
      DROP COLUMN IF EXISTS "apple_id",
      DROP COLUMN IF EXISTS "avatar_url",
      DROP COLUMN IF EXISTS "provider_metadata",
      DROP COLUMN IF EXISTS "last_social_login";
    `);

    this.addSql(`
      ALTER TABLE "workshop"
      DROP COLUMN IF EXISTS "auth_provider",
      DROP COLUMN IF EXISTS "google_id",
      DROP COLUMN IF EXISTS "apple_id",
      DROP COLUMN IF EXISTS "avatar_url",
      DROP COLUMN IF EXISTS "provider_metadata",
      DROP COLUMN IF EXISTS "last_social_login";
    `);
  }
}





