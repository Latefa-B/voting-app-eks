resource "aws_security_group" "eks_cluster_sg" {
  name        = "${var.cluster_name}-cluster-sg"
  description = "EKS cluster security group"
  vpc_id      = aws_vpc.main.id

  # Allow Kubernetes API access
  ingress {
    description = "Kubernetes API"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"

    # IMPORTANT: for project scope we keep it open
    # (you can restrict later to your IP)
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.cluster_name}-cluster-sg"
  }
}


resource "aws_security_group" "eks_node_sg" {
  name        = "${var.cluster_name}-node-sg"
  description = "EKS worker node security group"
  vpc_id      = aws_vpc.main.id

  # Node-to-node communication (required for Kubernetes networking)
  ingress {
    description = "Self communication"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    self        = true
  }

  # Allow cluster control plane to talk to nodes
  ingress {
    description     = "Cluster to nodes"
    from_port       = 1025
    to_port         = 65535
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_cluster_sg.id]
  }

  # Optional SSH for debugging (can remove later)
  ingress {
    description = "SSH access"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.cluster_name}-node-sg"
  }
}
