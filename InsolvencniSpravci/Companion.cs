using System;

namespace InsolvencniSpravci
{
	class Companion
	{
		public int AdministratorId { get; set; }
		public string Name { get; set; }
		public string Surename { get; set; }
		public DateTime? BirthDate { get; set; }
		public bool CanDebtRelief { get; set; }
		public bool CanKonkurs { get; set; }
		public DateTime? DateOfSpecialExamination { get; set; }
	}
}
