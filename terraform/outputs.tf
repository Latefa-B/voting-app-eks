output "cluster_name"   { value = aws_eks_cluster.main.name }
output "ecr_vote_url"   { value = aws_ecr_repository.app["vote"].repository_url }
output "ecr_result_url" { value = aws_ecr_repository.app["result"].repository_url }
output "ecr_worker_url" { value = aws_ecr_repository.app["worker"].repository_url }
output "aws_account_id" { value = data.aws_caller_identity.current.account_id }
