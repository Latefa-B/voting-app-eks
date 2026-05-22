variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "cluster_name" {
  type    = string
  default = "voting-app-cluster"
}

variable "cluster_version" {
  type    = string
  default = "1.33"
}

variable "node_instance_type" {
  type    = string
  default = "t3.medium"
}

variable "node_desired_size" {
  type    = number
  default = 2
}

variable "node_min_size" {
  type    = number
  default = 1
}

variable "node_max_size" {
  type    = number
  default = 4
}

variable "project_name" {
  type    = string
  default = "voting-app"
}
