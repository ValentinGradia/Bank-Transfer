terraform {
  required_version = ">= 1.2"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm",
      version = "~> 4.2"
    }
  }
}

//Definimos el servicio en la nube -> Con que nube trabajo
provider "azurerm" {
  resource_provider_registrations = "none"
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

//MS -> Microservicio

//Resource group -> Carpeta / contenedor logico
//Los resources primero tienen el nombre del tipo de recurso, esto indicandole a terraform que recurso queremos crear.
//Y segundo es el identificador interno.
//Definimos los recursos especificos a crear -> Que recurso creo o administro en la nube
resource "azurerm_resource_group" "rg" {
	name     = "rg-transfer-eastus"
	location = "eastus"
}


//El service plan especifica donde y como se ejecutara nuestro MS. Le indicamos al web app
//corre dentro de este plan de servicio -> Lo vamos a usar siempre que queramos corer una app web en azure

//El nuestro MS que se ejecuta en azure. Basicamente, el recurso que aloja nuestro codigo
# service_plan	Define QUÉ máquina usar (recursos, costo)
# linux_web_app	Define DÓNDE y CÓMO corre tu app

resource "azurerm_service_plan" "plan_apigateaway" {
	name                = "plan-apigateaway-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	os_type             = "Linux"
	sku_name            = "F1"
}

//MS Gateaway
resource "azurerm_linux_web_app" "appservice_gateaway" {
	name                = "appserv-apigateaway-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	service_plan_id     = azurerm_service_plan.plan_apigateaway.id
	site_config {
		always_on = false //Ambiente de pruebas, optamos que este apagado cuando no hay interaccion
		application_stack {
		docker_image_name = "nginx:latest"
		}
	}
}


//MS Transaction
resource "azurerm_linux_web_app" "appservice_transaction" {
	name                = "appserv-Mytransaction-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	service_plan_id     = azurerm_service_plan.plan_apigateaway.id
	site_config {
		always_on = false //Ambiente de pruebas, optamos que este apagado cuando no hay interaccion
		application_stack {
		docker_image_name = "nginx:latest"
		}
	}
}


//MS Balance
resource "azurerm_linux_web_app" "appservice_balance" {
	name                = "appserv-My-balance-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	service_plan_id     = azurerm_service_plan.plan_apigateaway.id
	site_config {
		always_on = false //Ambiente de pruebas, optamos que este apagado cuando no hay interaccion
		application_stack {
		docker_image_name = "nginx:latest"
		}
	}
}

//MS Transfer
resource "azurerm_linux_web_app" "appservice_transfer" {
	name                = "appserv-My-transfer-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	service_plan_id     = azurerm_service_plan.plan_apigateaway.id
	site_config {
		always_on = false //Ambiente de pruebas, optamos que este apagado cuando no hay interaccion
		application_stack {
		docker_image_name = "nginx:latest"
		}
	}
}

//MS Notification
resource "azurerm_linux_web_app" "appservice_notification" {
	name                = "appserv-my-notification-centralus"
	location            = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	service_plan_id     = azurerm_service_plan.plan_apigateaway.id
	site_config {
		always_on = false //Ambiente de pruebas, optamos que este apagado cuando no hay interaccion
		application_stack {
		docker_image_name = "nginx:latest"
		}
	}
}

//DATABASE
resource "azurerm_mssql_server" "sql_my_server" {
	name = "mssql-server-centralus"
	location = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	version = "12.0"
	administrator_login = "adminuser"
	administrator_login_password = "Pass.12345@"
}

resource "azurerm_mssql_database" "sql_my_database" {
	name = "sql-my-db-centralus"
	server_id = azurerm_mssql_server.sql_my_server.id
	sku_name = "Basic"
}

//DB Cosmos DB
resource "azurerm_cosmosdb_account" "cosmosdb_account_notification" {
	name = "account-notification-v2-centralus"
	location = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	offer_type = "Standard"
	free_tier_enabled = true
	consistency_policy {
		consistency_level = "Session" //Garantizar que un cliente puede leer su ultima escritura
	}
	geo_location {
		location = "centralus"
		failover_priority = 0 
	}
}

resource "azurerm_cosmosdb_sql_database" "cosmosdb_sql_notification" {
	name = "cosmosdb_notification"
	resource_group_name = azurerm_resource_group.rg.name
	account_name = azurerm_cosmosdb_account.cosmosdb_account_notification.name
}


//Azure storage account
resource "azurerm_storage_account" "storage_account_function" {
	name = "safunctioncentralus"
	resource_group_name = azurerm_resource_group.rg.name
	location = "centralus"
	account_tier   = "Standard"
	account_replication_type = "LRS"
}

//Azure Service bus
resource "azurerm_servicebus_namespace" "sb_transfer" {
	name = "servicebus-transfer-centralus-v2"
	location = "centralus"
	resource_group_name = azurerm_resource_group.rg.name
	sku = "Standard"
}



data "azurerm_client_config" "current"{}


