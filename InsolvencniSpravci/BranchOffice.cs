namespace InsolvencniSpravci
{
	class BranchOffice
	{
		public int AdministratorId { get; set; }
		public string Address { get; set; }
		public string District { get; set; }
		public string Region { get; set; }
		public bool IsKonkursBranch { get; set; }
	}
}
