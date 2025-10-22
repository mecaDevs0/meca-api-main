# Makefile para facilitar comandos Docker

.PHONY: help build up down logs restart clean deploy dev test

help: ## Mostra esta ajuda
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

build: ## Build da imagem Docker
	docker-compose build

up: ## Inicia containers
	docker-compose up -d

down: ## Para containers
	docker-compose down

logs: ## Mostra logs
	docker-compose logs -f meca-api

restart: ## Restart containers
	docker-compose restart

clean: ## Remove containers, volumes e imagens
	docker-compose down -v
	docker system prune -f

deploy: ## Deploy para EC2
	./deploy-docker.sh

dev: ## Inicia ambiente de desenvolvimento
	docker-compose -f docker-compose.dev.yml up

test: ## Testa API
	curl -s http://localhost:9000/health | python3 -m json.tool

shell: ## Abre shell no container
	docker-compose exec meca-api sh

rebuild: ## Rebuild completo
	docker-compose down
	docker-compose build --no-cache
	docker-compose up -d

status: ## Status dos containers
	docker-compose ps
	@echo "\n📊 Health status:"
	@curl -s http://localhost:9000/health 2>/dev/null | python3 -m json.tool || echo "API não está respondendo"


